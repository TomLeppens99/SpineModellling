# -*- coding: utf-8 -*-
"""
Created on Tue Jun 23 09:59:42 2020

@author: andre
"""

import re
import numpy as np

def pelvicIncidence(s, hip):
    x_m_s1, y_m_s1 = (s[0] + s[2]) / 2, (s[1] + s[3]) / 2
    x_m_hip, y_m_hip = (hip[0] + hip[2]) / 2, (hip[1] + hip[3]) / 2
    
    m = (s[3] - s[1]) / (s[2] - s[0])
    m1 = -1/m
    m2 = (y_m_s1 - y_m_hip) / (x_m_s1 - x_m_hip)
    
    alpha = np.arctan(np.fabs((m1 - m2) / (1 + m1*m2)))
    
    return alpha*180/np.pi

def pelvicTilt(s, hip):
    x_m_s1, y_m_s1 = (s[0] + s[2]) / 2, (s[1] + s[3]) / 2
    x_m_hip, y_m_hip = (hip[0] + hip[2]) / 2, (hip[1] + hip[3]) / 2
    
    m1 = (y_m_hip - y_m_s1) / (x_m_hip - x_m_s1)
    m2 = 0
    
    #alpha = np.arctan(np.fabs((m1 - m2) / (1 + m1*m2)))
    alpha = np.arctan(np.fabs(m1))
    
    if x_m_s1 < x_m_hip:
        alpha = -(90 - alpha*180/np.pi)
    elif x_m_s1 > x_m_hip:
        alpha = 90 - alpha*180/np.pi
    else:
        alpha = 0
    
    return alpha

def anglesCalculation(coord_1, coord_2, ss = False):
    if ss:
        m = (coord_1[1] - coord_1[3]) / (coord_1[0] - coord_1[2])
        alpha = np.arctan(np.fabs(m))
    else:
        m1 = (coord_1[1] - coord_1[3]) / (coord_1[0] - coord_1[2])
        m2 = (coord_2[1] - coord_2[3]) / (coord_2[0] - coord_2[2])
    
        alpha = np.arctan(np.fabs((m1 - m2) / (1 + m1*m2)))   
    
    return alpha*180/np.pi

def angles_from_file(file):    
    for i in file:
        T4_sup_post = re.search('T4,Plat_Sup_Post,', i)
        T4_sup_ant = re.search('T4,Plat_Sup_Ant,', i)
        T12_inf_post = re.search('T12,Plat_Inf_Post,', i)
        T12_inf_ant = re.search('T12,Plat_Inf_Ant,', i)
        L1_sup_post = re.search('L1,Plat_Sup_Post,', i)
        L1_sup_ant = re.search('L1,Plat_Sup_Ant,', i)
        L5_inf_post = re.search('L5,Plat_Inf_Post,', i)
        L5_inf_ant = re.search('L5,Plat_Inf_Ant,', i)     
        S1_sup_post = re.search('S1,Plat_Sup_Post,', i)
        S1_sup_ant = re.search('S1,Plat_Sup_Ant,', i)
        Hip_G = re.search('Hips,Hip_G,', i)
        Hip_D = re.search('Hips,Hip_D,', i)
        
        if T4_sup_post:
            T4_post = i[T4_sup_post.end():]
            T4_post = re.findall('\d+\.\d+', T4_post)
            T4_post = list(map(float, T4_post))
        
        elif T4_sup_ant:
            T4_ant = i[T4_sup_ant.end():]
            T4_ant = re.findall('\d+\.\d+', T4_ant)
            T4_ant = list(map(float, T4_ant))
            
        elif T12_inf_post:
            T12_post = i[T12_inf_post.end():]
            T12_post = re.findall('\d+\.\d+', T12_post) 
            T12_post = list(map(float, T12_post))
        
        elif T12_inf_ant:
            T12_ant = i[T12_inf_ant.end():]
            T12_ant = re.findall('\d+\.\d+', T12_ant)
            T12_ant = list(map(float, T12_ant))

        elif L1_sup_post:
            L1_post = i[L1_sup_post.end():]
            L1_post = re.findall('\d+\.\d+', L1_post)
            L1_post = list(map(float, L1_post))
        
        elif L1_sup_ant:
            L1_ant = i[L1_sup_ant.end():]
            L1_ant = re.findall('\d+\.\d+', L1_ant)
            L1_ant = list(map(float, L1_ant))

        elif L5_inf_post:
            L5_post = i[L5_inf_post.end():]
            L5_post = re.findall('\d+\.\d+', L5_post)
            L5_post = list(map(float, L5_post))
        elif L5_inf_ant:
            L5_ant = i[L5_inf_ant.end():]
            L5_ant = re.findall('\d+\.\d+', L5_ant)
            L5_ant = list(map(float, L5_ant))                   
        
        elif S1_sup_post:
            S1_post = i[S1_sup_post.end():]
            S1_post = re.findall('\d+\.\d+', S1_post)
            S1_post = list(map(float, S1_post))
            
        elif S1_sup_ant:
            S1_ant = i[S1_sup_ant.end():]
            S1_ant = re.findall('\d+\.\d+', S1_ant)
            S1_ant = list(map(float, S1_ant))

        elif Hip_G:
            Hip_g = i[Hip_G.end():]
            Hip_g = re.findall('\d+\.\d+', Hip_g)
            Hip_g = list(map(float, Hip_g))
        
        elif Hip_D:
            Hip_d = i[Hip_D.end():]
            Hip_d = re.findall('\d+\.\d+', Hip_d)
            Hip_d = list(map(float, Hip_d))
            
            
            
    L5_coords = L5_post[0], L5_post[2], L5_ant[0], L5_ant[2]
    L1_coords = L1_post[0], L1_post[2], L1_ant[0], L1_ant[2]
    S1_coords = S1_post[0], S1_post[2], S1_ant[0], S1_ant[2]
    T4_coords = T4_post[0], T4_post[2], T4_ant[0], T4_ant[2]
    T12_coords = T12_post[0], T12_post[2], T12_ant[0], T12_ant[2]
    Hip_coords = Hip_d[0], Hip_d[2], Hip_g[0], Hip_g[2]
    
    L1_L5 = anglesCalculation(L1_coords, L5_coords)
    L1_S1 = anglesCalculation(L1_coords, S1_coords)
    T4_T12 = anglesCalculation(T4_coords, T12_coords)
    SS = anglesCalculation(S1_coords, S1_coords, ss = True)
    PI = pelvicIncidence(S1_coords, Hip_coords)
    PT = pelvicTilt(S1_coords, Hip_coords)
        
    return L1_L5, L1_S1, T4_T12, SS, PI, PT

def calculate_m(coords):
    return (coords[1] - coords[3]) / (coords[0] - coords[2])

def Cobb(m_sup, m_inf, vertebrae):
    cobb = 0
    for i in range(len(m_sup)):
        for j in range(i+1, len(m_inf)):
            alpha = np.arctan((m_sup[i] - m_inf[j]) / (1 + m_sup[i]*m_inf[j]))
            alpha = np.fabs(alpha*180/np.pi)
            if alpha > cobb:
                cobb = alpha
                v_sup, v_inf = vertebrae[i], vertebrae[j]
    return cobb, str(v_sup + '-' + v_inf)
        
def coordinates(file):
    coords_sup = []
    coords_inf = []
    for i in file:
        sup_D = re.search('Plat_Sup_D,', i)
        sup_G = re.search('Plat_Sup_G,', i)
        inf_D = re.search('Plat_Inf_G,', i)
        inf_G = re.search('Plat_Inf_D,', i)
        if sup_G:        
            sup_g = i[sup_G.end():]       
            sup_g = re.findall('\d+\.\d+', sup_g)      
            sup_g = list(map(float, sup_g))
            coords_sup.append(sup_g[1:3])
        elif sup_D:
            sup_d = i[sup_D.end():]
            sup_d = re.findall('\d+\.\d+', sup_d)        
            sup_d = list(map(float, sup_d))
            coords_sup.append(sup_d[1:3])
        elif inf_G:
            inf_g = i[inf_G.end():]
            inf_g = re.findall('\d+\.\d+', inf_g)
            inf_g = list(map(float, inf_g))
            coords_inf.append(inf_g[1:3])
        elif inf_D:
            inf_d = i[inf_D.end():]
            inf_d = re.findall('\d+\.\d+', inf_d)
            inf_d = list(map(float, inf_d))
            coords_inf.append(inf_d[1:3])
    '''coords_sup.reverse()
                coords_inf.reverse()'''
    return coords_sup, coords_inf
         

def coefficients(coords_sup, coords_inf):
    coords_sup_conc = []
    for i in range(0, len(coords_sup), 2):
        j = 0
        coords_sup_conc.append([coords_sup[i][j], coords_sup[i][j + 1], coords_sup[i + 1][j], coords_sup[i + 1][j + 1]])

    coords_inf_conc = []
    for i in range(0, len(coords_inf), 2):
        j = 0
        coords_inf_conc.append([coords_inf[i][j], coords_inf[i][j + 1], coords_inf[i + 1][j], coords_inf[i + 1][j + 1]])

    m_sup = []
    for i in coords_sup_conc:
        m_sup.append(calculate_m(i))

    m_inf = []
    for i in coords_inf_conc:
        m_inf.append(calculate_m(i))
        
    return m_sup, m_inf