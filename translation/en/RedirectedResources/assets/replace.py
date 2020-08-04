import os
from glob import glob

# build master dict
master = open("master.txt", "r")
master_dict = {}
for line in master:
    line = line.strip() # clear whitespaces
    if line[-1] != '=': # if it has a translation:
        key = line.split('=')[0]
        master_dict[key] = line

# build list of slaves
slave_list = [y for x in os.walk(os.getcwd()+'/list') for y in glob(os.path.join(x[0], '*.txt')) if 'master' not in y]

# iterate over list of slave files
for slave_path in slave_list:
    slave = open(slave_path, "r")
    temp_file_contents = "" #temp string
    for slave_line in slave:
        print(slave_line)
        slave_line = slave_line.strip() # clear whitespaces
        jap_portion = slave_line.split('=')[0]
        try:
            temp_file_contents += master_dict[jap_portion] + '\n'
        except KeyError:
            temp_file_contents += slave_line + '\n'

    output_filepath = slave_path.split('.txt')[0]+'_output.txt'
    writing_file = open(output_filepath, "w")
    writing_file.write(temp_file_contents)
    writing_file.close()