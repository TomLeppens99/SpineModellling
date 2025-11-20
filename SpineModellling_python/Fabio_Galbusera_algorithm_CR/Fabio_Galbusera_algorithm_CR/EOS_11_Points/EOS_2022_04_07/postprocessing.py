import torch

def resize_coordinates(coordinates, size, lat = False):
    #height and width of the original image
    h, w = 1024, 1024
    
    w = w / 2

    #new size
    new_h, new_w = size[0], size[1]

    new_h, new_w = int(new_h), int(new_w)

    #find the new coordinates in the new dimensions
    if lat:
        coordinates[:, 0] = coordinates[:, 0] - w
        
    landmarks = coordinates * torch.tensor([new_w / w, new_h / h])
    return landmarks