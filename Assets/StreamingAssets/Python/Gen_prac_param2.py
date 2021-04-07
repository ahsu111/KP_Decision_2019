import random

lst = []

for j in [5,6,7]:
    for i in range(10):
        lst.append(random.randint(10**(j-1),10**(j)))

random.shuffle(lst)

print(lst)
from random import shuffle

prob_type = input("Enter type of problem, can be 'cost', 'reward' or 'size': ")

number_of_unique_instances = 4
number_of_instance_files = 150

saccade_trials_per_block = 12
Saccade_blocks = 2



for j in range(1, number_of_instance_files + 1):
    f = open("%r_param2.txt" % j,"w+")
    if prob_type == "cost":
        reward_level = [0.5]
        
        x = [i for i in range(1,number_of_unique_instances + 1)]
        print(x)
    elif prob_type == "reward":
        reward_level = [0.01, 1, 2]
        
        x = [[i,reward_level[j]] for i in range(1,number_of_unique_instances + 1) for j in range(len(reward_level))]
        print(x)
    elif prob_type == "size":
        reward_level = [1.5]
        x = [i for i in range(1,number_of_unique_instances + 1)]
        print(x)

    x = x*3
    
    shuffle(x)

    ITI = [5,6,7] * 4

    SaccadeITI = [0.7,1,1.3] * 8
    
    f.write("decision:1\n")

    if prob_type == "cost":
        f.write("cost:1\n")
        f.write("cost_digits:[" + ",".join(str(num) for num in lst) + "]\n")
        
        f.write("reward_amount:[" + ",".join(str(reward_level[0]) for num in lst) + "]\n")

        f.write("ITI:[" + ",".join(str(num) for num in ITI) + "]\n")
        
        f.write("SaccadeITI:[" + ",".join(str(num) for num in SaccadeITI) + "]\n")
        
        f.write("size:0\n")
        f.write("reward:0\n")
        
        f.write("numberOfTrials:6\n")
        f.write("numberOfBlocks:2\n")
        
        f.write("numberOfInstances:4\n")
        
        KP = "instanceRandomization:[" + ",".join(str(num) for num in x) + "]\n"
        f.write(KP)

    elif prob_type == "reward":
        f.write("reward:1\n")
        f.write("reward_amount:[" + ",".join(str(num[1]) for num in x) + "]\n")
    
        f.write("numberOfTrials:12\n")
        f.write("numberOfBlocks:6\n")

        f.write("numberOfInstances:24\n")
        KP = "instanceRandomization:[" + ",".join(str(num[0]) for num in x) + "]\n"
        print(KP)
        f.write(KP)
    
    elif prob_type == "size":
        f.write("decision:1\n")
    
        reward_list = [reward_level[0] for i in range(number_of_unique_instances)]
        
        f.write("reward_amount:[" + ",".join(str(num) for num in reward_list) + "]\n")
        
        
        f.write("size:1\n")

        f.write("numberOfTrials:8\n")
        f.write("numberOfBlocks:2\n")
        
        f.write("numberOfInstances:16\n")
        KP = "instanceRandomization:[" + ",".join(str(num) for num in x) + "]\n"
        f.write(KP)




    if prob_type == "cost" or prob_type == "reward":
        f.write("numberOfSaccadeTrials:%d\n" % saccade_trials_per_block)
        f.write("numberOfSaccadeBlocks:2\n")

        Saccade_randomisation = []
        for i in range(Saccade_blocks):
            Saccade_randomisation_temp = [0, 1, 2, 3] * 3
            shuffle(Saccade_randomisation_temp)
            Saccade_randomisation = Saccade_randomisation + Saccade_randomisation_temp
            
        SR = "SaccadeRandomization:[" + ",".join(str(num) for num in Saccade_randomisation) + "]\n"
        print(SR)
        f.write(SR)
    
    f.close()
