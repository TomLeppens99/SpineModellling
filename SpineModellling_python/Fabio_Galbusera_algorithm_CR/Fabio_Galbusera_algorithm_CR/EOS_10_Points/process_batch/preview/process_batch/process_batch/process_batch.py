import os
import network
import preprocessing
import postprocessing
import matplotlib.pyplot as plt
import torch
import dsntnn
from calculate_angles import *
from view_images import *

#data_folder = '/mnt/69d0f83d-78c6-402f-9ab7-5cab01bde5b8/EOS_jpgs/'
data_folder = 'C:/Users/raube/Nextcloud/MSKSpineModelingAIS/Fabio_Galbusera_algorithm_CR/process_batch/preview/sample_inputs_EOS/'


model = network.CoordRegressionNetwork(n_locations = 756, in_channels = 128)
model.load_state_dict(torch.load('best_train_all_stacked.pth', map_location={'cuda:0': 'cpu'}))
#model.load_state_dict(torch.load('best_train_all_stacked.pth'))
model.eval()

for root, dirs, files in os.walk(data_folder):
    for name_c in files:
        if name_c[-6:] == '_C.jpg':
            print
            name_l = name_c[:-6] + '_L.jpg'
            if os.path.isfile(data_folder + name_l):
                print("couple found: {} {}\n".format(name_c, name_l))

                image, size_ap, size_lat = preprocessing.ReadImages().preprocess_bynames(data_folder + name_c, data_folder + name_l)

                image = image.unsqueeze(dim = 0)

                coords, _ = model(image)
                
                coords = coords[-1]
                
                landmarks_pred_retransformed = dsntnn.normalized_to_pixel_coordinates(coords, (image.shape[2], image.shape[3]))

                landmarks_pred = landmarks_pred_retransformed.squeeze()

                ind_antero = list(range(1, len(landmarks_pred), 2))
                ind_lateral = list(range(0, len(landmarks_pred), 2))
                
                landmarks_pred_ap = landmarks_pred[ind_antero, :]
                landmarks_pred_lat = landmarks_pred[ind_lateral, :]

                landmarks_ap = postprocessing.resize_coordinates(coordinates = landmarks_pred_ap.cpu(), size = size_ap, lat = False)
                landmarks_lat = postprocessing.resize_coordinates(coordinates = landmarks_pred_lat.cpu(), size = size_lat, lat = True)

                labels = ['L5,Plat_Sup_G,',
                         'L5,Plat_Sup_Cent,',
                         'L5,Plat_Sup_D,',
                         'L5,Plat_Sup_Ant,',
                         'L5,Plat_Sup_Post,',
                         'L5,Plat_Inf_G,',
                         'L5,Plat_Inf_Cent,',
                         'L5,Plat_Inf_D,',
                         'L5,Plat_Inf_Ant,',
                         'L5,Plat_Inf_Post,',
                         'L5,Plat_Cent_G,',
                         'L5,Plat_Cent_Cent,',
                         'L5,Plat_Cent_D,',
                         'L5,Plat_Cent_Ant,',
                         'L5,Ped_Inf_G,',
                         'L5,Ped_Inf_D,',
                         'L5,Ped_Sup_G,',
                         'L5,Ped_Sup_D,',
                         'L5,Ped_G_Ext,',
                         'L5,Ped_G_Int,',
                         'L5,Ped_D_Ext,',
                         'L5,Ped_D_Int,',
                         'L4,Plat_Sup_G,',
                         'L4,Plat_Sup_Cent,',
                         'L4,Plat_Sup_D,',
                         'L4,Plat_Sup_Ant,',
                         'L4,Plat_Sup_Post,',
                         'L4,Plat_Inf_G,',
                         'L4,Plat_Inf_Cent,',
                         'L4,Plat_Inf_D,',
                         'L4,Plat_Inf_Ant,',
                         'L4,Plat_Inf_Post,',
                         'L4,Plat_Cent_G,',
                         'L4,Plat_Cent_Cent,',
                         'L4,Plat_Cent_D,',
                         'L4,Plat_Cent_Ant,',
                         'L4,Ped_Inf_G,',
                         'L4,Ped_Inf_D,',
                         'L4,Ped_Sup_G,',
                         'L4,Ped_Sup_D,',
                         'L4,Ped_G_Ext,',
                         'L4,Ped_G_Int,',
                         'L4,Ped_D_Ext,',
                         'L4,Ped_D_Int,',
                         'L3,Plat_Sup_G,',
                         'L3,Plat_Sup_Cent,',
                         'L3,Plat_Sup_D,',
                         'L3,Plat_Sup_Ant,',
                         'L3,Plat_Sup_Post,',
                         'L3,Plat_Inf_G,',
                         'L3,Plat_Inf_Cent,',
                         'L3,Plat_Inf_D,',
                         'L3,Plat_Inf_Ant,',
                         'L3,Plat_Inf_Post,',
                         'L3,Plat_Cent_G,',
                         'L3,Plat_Cent_Cent,',
                         'L3,Plat_Cent_D,',
                         'L3,Plat_Cent_Ant,',
                         'L3,Ped_Inf_G,',
                         'L3,Ped_Inf_D,',
                         'L3,Ped_Sup_G,',
                         'L3,Ped_Sup_D,',
                         'L3,Ped_G_Ext,',
                         'L3,Ped_G_Int,',
                         'L3,Ped_D_Ext,',
                         'L3,Ped_D_Int,',
                         'L2,Plat_Sup_G,',
                         'L2,Plat_Sup_Cent,',
                         'L2,Plat_Sup_D,',
                         'L2,Plat_Sup_Ant,',
                         'L2,Plat_Sup_Post,',
                         'L2,Plat_Inf_G,',
                         'L2,Plat_Inf_Cent,',
                         'L2,Plat_Inf_D,',
                         'L2,Plat_Inf_Ant,',
                         'L2,Plat_Inf_Post,',
                         'L2,Plat_Cent_G,',
                         'L2,Plat_Cent_Cent,',
                         'L2,Plat_Cent_D,',
                         'L2,Plat_Cent_Ant,',
                         'L2,Ped_Inf_G,',
                         'L2,Ped_Inf_D,',
                         'L2,Ped_Sup_G,',
                         'L2,Ped_Sup_D,',
                         'L2,Ped_G_Ext,',
                         'L2,Ped_G_Int,',
                         'L2,Ped_D_Ext,',
                         'L2,Ped_D_Int,',
                         'L1,Plat_Sup_G,',
                         'L1,Plat_Sup_Cent,',
                         'L1,Plat_Sup_D,',
                         'L1,Plat_Sup_Ant,',
                         'L1,Plat_Sup_Post,',
                         'L1,Plat_Inf_G,',
                         'L1,Plat_Inf_Cent,',
                         'L1,Plat_Inf_D,',
                         'L1,Plat_Inf_Ant,',
                         'L1,Plat_Inf_Post,',
                         'L1,Plat_Cent_G,',
                         'L1,Plat_Cent_Cent,',
                         'L1,Plat_Cent_D,',
                         'L1,Plat_Cent_Ant,',
                         'L1,Ped_Inf_G,',
                         'L1,Ped_Inf_D,',
                         'L1,Ped_Sup_G,',
                         'L1,Ped_Sup_D,',
                         'L1,Ped_G_Ext,',
                         'L1,Ped_G_Int,',
                         'L1,Ped_D_Ext,',
                         'L1,Ped_D_Int,',
                         'T12,Plat_Sup_G,',
                         'T12,Plat_Sup_Cent,',
                         'T12,Plat_Sup_D,',
                         'T12,Plat_Sup_Ant,',
                         'T12,Plat_Sup_Post,',
                         'T12,Plat_Inf_G,',
                         'T12,Plat_Inf_Cent,',
                         'T12,Plat_Inf_D,',
                         'T12,Plat_Inf_Ant,',
                         'T12,Plat_Inf_Post,',
                         'T12,Plat_Cent_G,',
                         'T12,Plat_Cent_Cent,',
                         'T12,Plat_Cent_D,',
                         'T12,Plat_Cent_Ant,',
                         'T12,Ped_Inf_G,',
                         'T12,Ped_Inf_D,',
                         'T12,Ped_Sup_G,',
                         'T12,Ped_Sup_D,',
                         'T12,Ped_G_Ext,',
                         'T12,Ped_G_Int,',
                         'T12,Ped_D_Ext,',
                         'T12,Ped_D_Int,',
                         'T11,Plat_Sup_G,',
                         'T11,Plat_Sup_Cent,',
                         'T11,Plat_Sup_D,',
                         'T11,Plat_Sup_Ant,',
                         'T11,Plat_Sup_Post,',
                         'T11,Plat_Inf_G,',
                         'T11,Plat_Inf_Cent,',
                         'T11,Plat_Inf_D,',
                         'T11,Plat_Inf_Ant,',
                         'T11,Plat_Inf_Post,',
                         'T11,Plat_Cent_G,',
                         'T11,Plat_Cent_Cent,',
                         'T11,Plat_Cent_D,',
                         'T11,Plat_Cent_Ant,',
                         'T11,Ped_Inf_G,',
                         'T11,Ped_Inf_D,',
                         'T11,Ped_Sup_G,',
                         'T11,Ped_Sup_D,',
                         'T11,Ped_G_Ext,',
                         'T11,Ped_G_Int,',
                         'T11,Ped_D_Ext,',
                         'T11,Ped_D_Int,',
                         'T10,Plat_Sup_G,',
                         'T10,Plat_Sup_Cent,',
                         'T10,Plat_Sup_D,',
                         'T10,Plat_Sup_Ant,',
                         'T10,Plat_Sup_Post,',
                         'T10,Plat_Inf_G,',
                         'T10,Plat_Inf_Cent,',
                         'T10,Plat_Inf_D,',
                         'T10,Plat_Inf_Ant,',
                         'T10,Plat_Inf_Post,',
                         'T10,Plat_Cent_G,',
                         'T10,Plat_Cent_Cent,',
                         'T10,Plat_Cent_D,',
                         'T10,Plat_Cent_Ant,',
                         'T10,Ped_Inf_G,',
                         'T10,Ped_Inf_D,',
                         'T10,Ped_Sup_G,',
                         'T10,Ped_Sup_D,',
                         'T10,Ped_G_Ext,',
                         'T10,Ped_G_Int,',
                         'T10,Ped_D_Ext,',
                         'T10,Ped_D_Int,',
                         'T9,Plat_Sup_G,',
                         'T9,Plat_Sup_Cent,',
                         'T9,Plat_Sup_D,',
                         'T9,Plat_Sup_Ant,',
                         'T9,Plat_Sup_Post,',
                         'T9,Plat_Inf_G,',
                         'T9,Plat_Inf_Cent,',
                         'T9,Plat_Inf_D,',
                         'T9,Plat_Inf_Ant,',
                         'T9,Plat_Inf_Post,',
                         'T9,Plat_Cent_G,',
                         'T9,Plat_Cent_Cent,',
                         'T9,Plat_Cent_D,',
                         'T9,Plat_Cent_Ant,',
                         'T9,Ped_Inf_G,',
                         'T9,Ped_Inf_D,',
                         'T9,Ped_Sup_G,',
                         'T9,Ped_Sup_D,',
                         'T9,Ped_G_Ext,',
                         'T9,Ped_G_Int,',
                         'T9,Ped_D_Ext,',
                         'T9,Ped_D_Int,',
                         'T8,Plat_Sup_G,',
                         'T8,Plat_Sup_Cent,',
                         'T8,Plat_Sup_D,',
                         'T8,Plat_Sup_Ant,',
                         'T8,Plat_Sup_Post,',
                         'T8,Plat_Inf_G,',
                         'T8,Plat_Inf_Cent,',
                         'T8,Plat_Inf_D,',
                         'T8,Plat_Inf_Ant,',
                         'T8,Plat_Inf_Post,',
                         'T8,Plat_Cent_G,',
                         'T8,Plat_Cent_Cent,',
                         'T8,Plat_Cent_D,',
                         'T8,Plat_Cent_Ant,',
                         'T8,Ped_Inf_G,',
                         'T8,Ped_Inf_D,',
                         'T8,Ped_Sup_G,',
                         'T8,Ped_Sup_D,',
                         'T8,Ped_G_Ext,',
                         'T8,Ped_G_Int,',
                         'T8,Ped_D_Ext,',
                         'T8,Ped_D_Int,',
                         'T7,Plat_Sup_G,',
                         'T7,Plat_Sup_Cent,',
                         'T7,Plat_Sup_D,',
                         'T7,Plat_Sup_Ant,',
                         'T7,Plat_Sup_Post,',
                         'T7,Plat_Inf_G,',
                         'T7,Plat_Inf_Cent,',
                         'T7,Plat_Inf_D,',
                         'T7,Plat_Inf_Ant,',
                         'T7,Plat_Inf_Post,',
                         'T7,Plat_Cent_G,',
                         'T7,Plat_Cent_Cent,',
                         'T7,Plat_Cent_D,',
                         'T7,Plat_Cent_Ant,',
                         'T7,Ped_Inf_G,',
                         'T7,Ped_Inf_D,',
                         'T7,Ped_Sup_G,',
                         'T7,Ped_Sup_D,',
                         'T7,Ped_G_Ext,',
                         'T7,Ped_G_Int,',
                         'T7,Ped_D_Ext,',
                         'T7,Ped_D_Int,',
                         'T6,Plat_Sup_G,',
                         'T6,Plat_Sup_Cent,',
                         'T6,Plat_Sup_D,',
                         'T6,Plat_Sup_Ant,',
                         'T6,Plat_Sup_Post,',
                         'T6,Plat_Inf_G,',
                         'T6,Plat_Inf_Cent,',
                         'T6,Plat_Inf_D,',
                         'T6,Plat_Inf_Ant,',
                         'T6,Plat_Inf_Post,',
                         'T6,Plat_Cent_G,',
                         'T6,Plat_Cent_Cent,',
                         'T6,Plat_Cent_D,',
                         'T6,Plat_Cent_Ant,',
                         'T6,Ped_Inf_G,',
                         'T6,Ped_Inf_D,',
                         'T6,Ped_Sup_G,',
                         'T6,Ped_Sup_D,',
                         'T6,Ped_G_Ext,',
                         'T6,Ped_G_Int,',
                         'T6,Ped_D_Ext,',
                         'T6,Ped_D_Int,',
                         'T5,Plat_Sup_G,',
                         'T5,Plat_Sup_Cent,',
                         'T5,Plat_Sup_D,',
                         'T5,Plat_Sup_Ant,',
                         'T5,Plat_Sup_Post,',
                         'T5,Plat_Inf_G,',
                         'T5,Plat_Inf_Cent,',
                         'T5,Plat_Inf_D,',
                         'T5,Plat_Inf_Ant,',
                         'T5,Plat_Inf_Post,',
                         'T5,Plat_Cent_G,',
                         'T5,Plat_Cent_Cent,',
                         'T5,Plat_Cent_D,',
                         'T5,Plat_Cent_Ant,',
                         'T5,Ped_Inf_G,',
                         'T5,Ped_Inf_D,',
                         'T5,Ped_Sup_G,',
                         'T5,Ped_Sup_D,',
                         'T5,Ped_G_Ext,',
                         'T5,Ped_G_Int,',
                         'T5,Ped_D_Ext,',
                         'T5,Ped_D_Int,',
                         'T4,Plat_Sup_G,',
                         'T4,Plat_Sup_Cent,',
                         'T4,Plat_Sup_D,',
                         'T4,Plat_Sup_Ant,',
                         'T4,Plat_Sup_Post,',
                         'T4,Plat_Inf_G,',
                         'T4,Plat_Inf_Cent,',
                         'T4,Plat_Inf_D,',
                         'T4,Plat_Inf_Ant,',
                         'T4,Plat_Inf_Post,',
                         'T4,Plat_Cent_G,',
                         'T4,Plat_Cent_Cent,',
                         'T4,Plat_Cent_D,',
                         'T4,Plat_Cent_Ant,',
                         'T4,Ped_Inf_G,',
                         'T4,Ped_Inf_D,',
                         'T4,Ped_Sup_G,',
                         'T4,Ped_Sup_D,',
                         'T4,Ped_G_Ext,',
                         'T4,Ped_G_Int,',
                         'T4,Ped_D_Ext,',
                         'T4,Ped_D_Int,',
                         'T3,Plat_Sup_G,',
                         'T3,Plat_Sup_Cent,',
                         'T3,Plat_Sup_D,',
                         'T3,Plat_Sup_Ant,',
                         'T3,Plat_Sup_Post,',
                         'T3,Plat_Inf_G,',
                         'T3,Plat_Inf_Cent,',
                         'T3,Plat_Inf_D,',
                         'T3,Plat_Inf_Ant,',
                         'T3,Plat_Inf_Post,',
                         'T3,Plat_Cent_G,',
                         'T3,Plat_Cent_Cent,',
                         'T3,Plat_Cent_D,',
                         'T3,Plat_Cent_Ant,',
                         'T3,Ped_Inf_G,',
                         'T3,Ped_Inf_D,',
                         'T3,Ped_Sup_G,',
                         'T3,Ped_Sup_D,',
                         'T3,Ped_G_Ext,',
                         'T3,Ped_G_Int,',
                         'T3,Ped_D_Ext,',
                         'T3,Ped_D_Int,',
                         'T2,Plat_Sup_G,',
                         'T2,Plat_Sup_Cent,',
                         'T2,Plat_Sup_D,',
                         'T2,Plat_Sup_Ant,',
                         'T2,Plat_Sup_Post,',
                         'T2,Plat_Inf_G,',
                         'T2,Plat_Inf_Cent,',
                         'T2,Plat_Inf_D,',
                         'T2,Plat_Inf_Ant,',
                         'T2,Plat_Inf_Post,',
                         'T2,Plat_Cent_G,',
                         'T2,Plat_Cent_Cent,',
                         'T2,Plat_Cent_D,',
                         'T2,Plat_Cent_Ant,',
                         'T2,Ped_Inf_G,',
                         'T2,Ped_Inf_D,',
                         'T2,Ped_Sup_G,',
                         'T2,Ped_Sup_D,',
                         'T2,Ped_G_Ext,',
                         'T2,Ped_G_Int,',
                         'T2,Ped_D_Ext,',
                         'T2,Ped_D_Int,',
                         'T1,Plat_Sup_G,',
                         'T1,Plat_Sup_Cent,',
                         'T1,Plat_Sup_D,',
                         'T1,Plat_Sup_Ant,',
                         'T1,Plat_Sup_Post,',
                         'T1,Plat_Inf_G,',
                         'T1,Plat_Inf_Cent,',
                         'T1,Plat_Inf_D,',
                         'T1,Plat_Inf_Ant,',
                         'T1,Plat_Inf_Post,',
                         'T1,Plat_Cent_G,',
                         'T1,Plat_Cent_Cent,',
                         'T1,Plat_Cent_D,',
                         'T1,Plat_Cent_Ant,',
                         'T1,Ped_Inf_G,',
                         'T1,Ped_Inf_D,',
                         'T1,Ped_Sup_G,',
                         'T1,Ped_Sup_D,',
                         'T1,Ped_G_Ext,',
                         'T1,Ped_G_Int,',
                         'T1,Ped_D_Ext,',
                         'T1,Ped_D_Int,',
                         'S1,Plat_Sup_Ant,',
                         'S1,Plat_Sup_Post,',
                         'Hips,Hip_G,',
                         'Hips,Hip_D,']

                out_filename = data_folder + name_c[:-6] + '.txt'
                out = open(out_filename, 'w')
                out.write('X: anteroposterior coordinate (lat image); Y: laterolateral coordinate (ap image); Z: craniocaudal coordinate (shared)')
                for j in range(landmarks_lat.shape[0]):
                    if j % 2 == 0:
                        out.write('{}{},{},{}\n'.format(labels[j], float(landmarks_lat[j, 0]), float(landmarks_ap[j, 0]), float((landmarks_lat[j, 1] + landmarks_ap[j, 1]) / 2)))
                    else:
                        out.write('{}{},{},{}\n'.format(labels[j], float(landmarks_lat[j, 0]), float(landmarks_ap[j, 0]), float((landmarks_lat[j, 1] + landmarks_ap[j, 1]) / 2)))
                out.close()

                try:
                    with open(out_filename, 'r') as f:
                        file = f.readlines()
                    L1_L5, L1_S1, T4_T12, SS, PI, PT = angles_from_file(file)
                    
                    c_sup, c_inf = coordinates(file)
                    
                    m_sup, m_inf = coefficients(c_sup, c_inf)
                    
                    cobb = Cobb(m_sup, m_inf)
                
                    coords_ap, coords_lat = read(file)
                    
                    img_ap = cv.imread(data_folder + name_c, cv.IMREAD_COLOR)
                    img_lat = cv.imread(data_folder + name_l, cv.IMREAD_COLOR)

                    img_tot = np.concatenate((img_ap, img_lat), axis = 1)

                    text = 'Cobb = {:.2f}\nL1-L5 = {:.2f}\nL1-S1 = {:.2f}\nT4-T12 = {:.2f}\nSS = {:.2f}\nPI = {:.2f}\nPT = {:.2f}'.format(cobb, L1_L5, L1_S1, T4_T12, SS, PI, PT)

                    props = dict(boxstyle = 'round', facecolor = 'wheat', alpha = 0.5)

                    fig = plt.figure(frameon=False)
                    fig.set_size_inches(10, 15)        
                    ax = plt.Axes(fig, [0., 0., 1., 1.])
                    ax.set_axis_off()
                    fig.add_axes(ax)        
                    ax.imshow(img_tot, aspect='auto')
                    ax.scatter(coords_ap[:, 0], coords_ap[:, 1], marker='.', color = 'y')
                    ax.scatter(coords_lat[:, 0] + img_ap.shape[1], coords_lat[:, 1], marker='.', color = 'y')
                    ax.text(0.05, 0.95, text, transform = ax.transAxes, fontsize = 14, verticalalignment = 'top', bbox = props)

                    processed_filename = data_folder + name_c[:-6] + '_processed.jpg'
                    plt.savefig(processed_filename, dpi=100)
                    plt.close()

                except:
                    print("Coupling error!\n")

