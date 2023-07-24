using System.Collections.Generic;
using UnityEngine;

public class RTX_Master : MonoBehaviour
{
    [Header("Global Settings")]
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;
    public Light DirectionalLight;

    [Range(1, 10)]
    public int ReflectionDepth = 8;

    [Header("Spheres")]
    public int SphereSeed;
    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 100;
    public float SpherePlacementRadius = 100.0f;


    private Camera _camera;
    private float _lastFieldOfView;
    private RenderTexture _target;
    private RenderTexture _converged;
    private Material _addMaterial;
    [HideInInspector]
    public uint _currentSample = 0;
    private ComputeBuffer _objectBuffer;
    public List<Transform> TransformsToWatch = new List<Transform>();

    private int  ObjectStride = 60;

    struct RTXMaterial{
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
        public float ior;

        public static RTXMaterial get_default(){
            RTXMaterial mat;
            mat.albedo = Vector3.zero;
            mat.specular = Vector3.zero;
            mat.smoothness = 1.0f;
            mat.emission = Vector3.zero;
            mat.ior = 0.0f;
            return mat;
        }
    }


    struct Object
    {
        public Vector3 position;
        public float radius;
        // public Matrix4x4 LocalToWorld;
        // public Matrix4x4 WorldToLocal;

        public RTXMaterial material;
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        TransformsToWatch.Add(transform);
        TransformsToWatch.Add(DirectionalLight.transform);
    }

    private void OnEnable()
    {
        _currentSample = 0;
        SetUpScene();
    }

    private void OnDisable()
    {
        if (_objectBuffer != null)
            _objectBuffer.Release();
    }

    private void Update()
    {
        foreach (Transform t in TransformsToWatch)
        {
            if (t.hasChanged)
            {
                _currentSample = 0;
                t.hasChanged = false;
            }
        }
    }

    private void SetUpScene()
    {
        Random.InitState(SphereSeed);
        List<Object> objects = new List<Object>();

        // Add a number of random spheres
        for (int i = 0; i < SpheresMax; i++)
        {
            Object sphere = new Object();
            sphere.material = RTXMaterial.get_default();

            // Radius and position
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            // Reject spheres that are intersecting others
            foreach (Object other in objects)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }
            
            // Albedo and specular color
            Color color = Random.ColorHSV();
            float chance = Random.value;
            if (chance < 1.0f)
            {
                bool metal = chance < 0.2f;
                sphere.material.albedo = metal ? Vector4.zero : new Vector4(color.r, color.g, color.b);
                sphere.material.specular = metal ? new Vector4(color.r, color.g, color.b) : Vector4.zero;
                sphere.material.smoothness = Random.value;
            }
            else
            {
                Color emission = Random.ColorHSV(0, 1, 0, 1, 3.0f, 8.0f);
                sphere.material.emission = new Vector3(emission.r, emission.g, emission.b);
            }

            // Add the sphere to the list
            objects.Add(sphere);

            SkipSphere:
            continue;
        }

        // Assign to compute buffer
        if (_objectBuffer != null)
            _objectBuffer.Release();
        if (objects.Count > 0)
        {
            _objectBuffer = new ComputeBuffer(objects.Count, ObjectStride);
            _objectBuffer.SetData(objects);
        }
    }

    private void SetShaderParameters()
    {
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetFloat("_Seed", Random.value);
        RayTracingShader.SetFloat("_ReflectionDepth", ReflectionDepth);

        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        if (_objectBuffer != null)
            RayTracingShader.SetBuffer(0, "_Objects", _objectBuffer);
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
            {
                _target.Release();
                _converged.Release();
            }

            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
            _converged = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _converged.enableRandomWrite = true;
            _converged.Create();

            // Reset sampling
            _currentSample = 0;
        }
    }

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit the result texture to the screen
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(_target, _converged, _addMaterial);
        Graphics.Blit(_converged, destination);
        _currentSample++;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }
}
