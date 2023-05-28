import numpy as np
import matplotlib.pyplot as plt
def generate_sin(freq,dur,amp):
    frequency = freq 
    duration = dur
    amplitude = amp  
    num_samples = 9000
    time = np.linspace(0, duration, num_samples)
    return amplitude * np.sin(2 * np.pi * frequency * time)

sin = generate_sin(150000,5,1) + generate_sin(10,5,1)

file = open("sin150k-10.csv","w")
for i in sin: 
    file.write(f"{i} \n")
file.close()