for i in range(1,51):
    with open(f"{i}_param2.txt") as f:
        lines = f.readlines()
        
        linenum = 0
        for l in lines:

            if linenum !=4:
                lines[linenum] = l
            else:
                lines[linenum] = "numberOfTrials:20\n"
                
            linenum = linenum + 1
            
        with open(f"{i}_param2.txt", "w") as f1:
            f1.writelines(lines)
            f1.close()
        f.close()
        
##for i in range(1,51):
##    with open(f"temp{i}_param2.txt") as f2:
##        lines = f2.readlines()
##        
##        linenum = 0
##        for l in lines:
##
##            if linenum !=4:
##                lines[linenum] = l
##            else:
##                lines[linenum] = "numberOfTrials:20\n"
##                
##            linenum = linenum + 1
##            
##        with open(f"temp{i}_param2.txt", "w") as f3:
##            f3.writelines(lines)
##
##            f3.close()
##            
##        f2.close()
