from random import shuffle

number_of_unique_instances = 16
number_of_instance_files = 25

reward_level = [0.1, 2, 8]

x = [[i,reward_level[j]] for i in range(1,number_of_unique_instances + 1) for j in range(len(reward_level))]
print(x)

for j in range(1, number_of_instance_files + 1):
    shuffle(x)
    
    f = open("%r_param2.txt" % j,"w+")

    f.write("decision:1\n")
    
    f.write("cost:0\n")
    f.write("cost_digits:4\n")

    f.write("reward:1\n")
    f.write("reward_amount:[" + ",".join(str(num[1]) for num in x) + "]\n")
    
    
    f.write("size:0\n")

    f.write("numberOfTrials:16\n")
    f.write("numberOfBlocks:3\n")

    f.write("numberOfInstances:16\n")

    #f.write("numberOfInstances:{}\n".format(number_of_unique_instances*len(reward_level)))

    KP = "instanceRandomization:[" + ",".join(str(num[0]) for num in x) + "]\n"
    print(KP)
    f.write(KP)

    f.close()
