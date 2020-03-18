# Code mostly taken from various stackoverflow answers and https://docs.python.org/2/library/xml.etree.elementtree.html
# Made changes to suit the Tobii xml file & Unity txt output
# Assumes shimmer data collection starts before Tobii data collection.

# Assumes shimmer data collection ends after Tobii data collection.

import xml.etree.ElementTree as ET
import csv
import time
import datetime
import csv


#shimmer_name = input("Please input the full name of the Shimmer output (e.g. EVCET20_test_Session1_Shimmer_7619_Calibrated_SD): ")
shimmer_name = "March20_Session2_Shimmer_7619_Calibrated_SD"
#file_prefix = input("Please input the Tobii output prefix (e.g. KP_ah01_1_12 March, 2020, 10-50): ")
file_prefix = "KP_1_1_05 March, 2020, 14-46"
print(file_prefix)

tree = ET.parse(file_prefix + "__Saccade_0.xml")
root = tree.getroot()



f = open('{}_combined_output.csv'.format(file_prefix), 'w')


data = []


with open('{}.csv'.format(shimmer_name), 'r') as csvfile:
    next(csvfile)
    next(csvfile)
    next(csvfile)
    reader = csv.reader(csvfile, delimiter=',')
    #reader = csv.reader(csvfile, delimiter='\t')
    for row in reader:
        data.append([val for val in row])
#print(data[-1][0])
        

    
csvwriter = csv.writer(f)

head = ['Shimmer_7619_Timestamp_Unix_CAL (ms)',
        'Shimmer_7619_GSR_Range_CAL (no_units)', 'Shimmer_7619_GSR_Skin_Conductance_CAL (uS)', 'Shimmer_7619_GSR_Skin_Resistance_CAL (kOhms)', 'Shimmer_7619_PPG_A13_CAL (mV)',
        'TimeStamp','SystemTime','SystemUNIXTime','LatestHitObject','ShowingSaccadeDot','SaccadeDotPosition','Left_GazePointOnDisplayArea_X','Left_GazePointOnDisplayArea_Y',
        'Left_GazePointOnDisplayArea_Valid','Left_PupilDiameter','Left_PupilDiameter_Valid','Right_GazePointOnDisplayArea_X','Right_GazePointOnDisplayArea_Y',
        'Right_GazePointOnDisplayArea_Valid','Right_PupilDiameter','Right_PupilDiameter_Valid']

csvwriter.writerow(head)


# find the starting timestamp for Tobii data
for att in root.findall('GazeData'):

    systime = att.attrib["SystemTime"]

    systime2 = datetime.datetime.strptime(systime, '%m/%d/%Y %H:%M:%S.%f')
    
    UNIX_time = time.mktime(systime2.timetuple())
    
    first_Tobii_time = (UNIX_time*1e3 + systime2.microsecond/1e3)

    break

# write all the shimmer data that occur before the first Tobii data
while True:
    curr_shimmer = data[0]
    curr_time = float(curr_shimmer[0])
    
    #print(curr_time, " ", first_Tobii_time)
    if (curr_time < first_Tobii_time):
        row = [float(i) for i in curr_shimmer[:-1]]

        csvwriter.writerow(row)

        del data[0]
    else:
        break
    
# Write Tobii data first, then find the first Shimmer data
for att in root.findall('GazeData'):
    attr_main = att.attrib
    #print(attr_main)
    row.append(attr_main["TimeStamp"])
    systime = attr_main["SystemTime"]

    systime2 = datetime.datetime.strptime(systime, '%m/%d/%Y %H:%M:%S.%f')
    
    UNIX_time = time.mktime(systime2.timetuple())
    #print(UNIX_time*1e3 + systime2.microsecond/1e3)
    UTC_time = UNIX_time*1e3 + systime2.microsecond/1e3
    
    curr_shimmer = data[0]
    curr_time = float(curr_shimmer[0])

    row = [float(i) for i in curr_shimmer[:-1]]

    if (curr_time < UTC_time):
        row = prev_row
        
        del data[0]
    else:
        prev_row = row.copy()
        

    row.append(systime)
    row.append(UTC_time)
    row.append(attr_main["LatestHitObject"])

    try:
        row.append(attr_main["ShowingSaccadeDot"])
        row.append(attr_main["SaccadeDotPosition"])
    except KeyError:
        row.append("N/A")
        row.append("N/A")

    
    for subatt in att.find('Left'):
        attr_left = subatt.attrib
        #print(attr_left)
        if (subatt.tag == "GazePointOnDisplayArea"):
            try:
                row.append(attr_left["Value"][1:-1].split(",")[0])
                row.append(attr_left["Value"][1:-1].split(",")[1])
            except:
                pass
        else:
            try:
                row.append(attr_left["Value"])
            except:
                pass
        
        try:    
            row.append(attr_left["Valid"])
        except:
            pass

        
    for subatt in att.find('Right'):
        attr_right = subatt.attrib
        #print(attr_right)
        if (subatt.tag == "GazePointOnDisplayArea"):
            try:
                row.append(attr_right["Value"][1:-1].split(",")[0])
                row.append(attr_right["Value"][1:-1].split(",")[1])
            except:
                pass
        else:
            try:
                row.append(attr_right["Value"])
            except:
                pass
            
        try:
            row.append(attr_right["Valid"])
        except:
            pass

    #print(row)
    csvwriter.writerow(row)


# write all the shimmer data that occur AFTER the first Tobii data
while True:
    try:
        curr_shimmer = data[0]
        curr_time = float(curr_shimmer[0])
    
        row = [float(i) for i in curr_shimmer[:-1]]

        csvwriter.writerow(row)

        del data[0]
    except:
        break


    
f.close()

