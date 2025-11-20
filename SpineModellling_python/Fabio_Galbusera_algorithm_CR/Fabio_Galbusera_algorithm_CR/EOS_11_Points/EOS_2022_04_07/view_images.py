# -*- coding: utf-8 -*-
"""
Created on Mon Jul 20 12:05:30 2020

@author: andre
"""
import re
import numpy as np
    
def read(file):
    coords = []
    for i in file:
        coords.append(i)
    
    coords_num = []
    for i in coords:
        coords_num.append(re.findall('\d+\.\d+', i))
    
    #coords_num = coords_num[1:] #in the first line I do not have coordinates
    coords_array = np.zeros((len(coords_num), len(coords_num[0])))
    for i in range(coords_array.shape[0]):
        for j in range(coords_array.shape[1]):
            coords_array[i, j] = float(coords_num[i][j])
      
    coords_ap = coords_array[:, [1, 2]]
    coords_lat = coords_array[:, [0, 2]]

    return coords_ap, coords_lat