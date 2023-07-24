from sympy import *
from time import time, sleep
import pickle
init_printing()



def getResult(equation_):
    resultMod = equation_.subs(R, R_)
    resultMod = resultMod.subs(r, r_)
    resultMod = resultMod.subs(ox, o[0])
    resultMod = resultMod.subs(oy, o[1])
    resultMod = resultMod.subs(oz, o[2])
    resultMod = resultMod.subs(dz, d[2])
    resultMod = resultMod.subs(dy, d[1])
    resultMod = resultMod.subs(dx, d[0])
    return simplify(resultMod).evalf()



startInit = time()

R, r, ox, oy, oz, dx, dy, dz, t = symbols("R r ox oy oz dx dy dz t")

o = [0,0,-7]
d = [0,0,1]
R_ = 3
r_ = 2


x = ox+dx*t
y = oy+dy*t
z = oz+dz*t


left = (x**2 + y**2 + z**2 + R**2 - r**2)**2
right = 4 * R**2 * (x**2 + y**2)

equation = left-right


stopInit = time()
print("Initializing took {} seconds.".format(stopInit-startInit))
startCalc = time()

result = solve(equation, t)

stopCalc = time()
print("Calculating took {} seconds.".format(stopCalc-startCalc))
print("This is the result:")
print(result)

f = open("Solution.pkl", "wb")
pickle.dump(result, f)
f.flush()
f.close()


print("Done!")
while(True): sleep(1)

