from random import shuffle

number_of_unique_instances = 16
number_of_instance_files = 25

reward_level = [1.5]

x = [i for i in range(1,number_of_unique_instances + 1)]
print(x)

for j in range(1, number_of_instance_files + 1):
    f = open("%r_param2.txt" % j,"w+")

    f.write("decision:1\n")
    
    f.write("cost:1\n")
    f.write("cost_digits:4\n")

    f.write("reward:0\n")
    reward_list = [reward_level[0] for i in range(number_of_unique_instances)]
    
    f.write("reward_amount:[" + ",".join(str(num) for num in reward_list) + "]\n")
    
    
    f.write("size:0\n")

    f.write("numberOfTrials:8\n")
    f.write("numberOfBlocks:2\n")
    
    f.write("numberOfInstances:16\n")

    shuffle(x)
    KP = "instanceRandomization:[" + ",".join(str(num) for num in x) + "]\n"
    print(KP)
    f.write(KP)

    f.close()
