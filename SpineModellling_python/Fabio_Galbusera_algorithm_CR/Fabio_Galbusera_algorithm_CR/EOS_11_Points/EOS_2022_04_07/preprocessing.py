import cv2 as cv
import numpy as np
import torch
import os
import pandas as pd

class ReadImages():
	def __init__(self, path, image_name):
		self.path = path
		self.image_name = image_name

	def read_images(self):
		image_anteroposterior = cv.imread(self.path + '/' + self.image_name, cv.IMREAD_COLOR)			
		image_lateral = cv.imread(self.path + '/' + os.path.splitext(os.path.basename(self.image_name))[0][:-1] + 'S' + os.path.splitext(os.path.basename(self.image_name))[1], cv.IMREAD_COLOR)
		
		if image_lateral == None:
			image_lateral = cv.imread(self.path + '/' + os.path.splitext(os.path.basename(self.image_name))[0][:-1] + 'L' + os.path.splitext(os.path.basename(self.image_name))[1], cv.IMREAD_COLOR)
		

		size_ap, size_lat = image_anteroposterior.shape, image_lateral.shape

		return self.resize(image_anteroposterior), self.resize(image_lateral), size_ap[:2], size_lat[:2]

	def concatenate(self, image_anteroposterior, image_lateral):
		return np.concatenate((image_anteroposterior, image_lateral), axis = 1)

	def resize(self, image, size = (1024, 512)):
	    #height and width of the original image
	    h, w = image.shape[:2]

	    #new size
	    new_h, new_w = size[0], size[1]

	    new_h, new_w = int(new_h), int(new_w)

	    #resize image
	    img = cv.resize(image, dsize = (new_w, new_h), interpolation = cv.INTER_CUBIC)
	    
	    return img

	def toTensor(self, image_concatenated):
		return torch.from_numpy(image_concatenated).permute(2, 0, 1).float()


	def normalize(self, image, resnet_norm):
		image = image.div(255.)
		if resnet_norm:
			#mean_x, mean_y, mean_z = torch.mean(image, axis = 1)
			mean_x, mean_y, mean_z = 0.485, 0.456, 0.406
			#std_x, std_y, std_z = torch.std(image, axis = 1)
			std_x, std_y, std_z = 0.229, 0.224, 0.225


			image[0, :, :] = (image[0, :, :] - mean_x) / std_x

			image[1, :, :] = (image[1, :, :] - mean_y) / std_y

			image[2, :, :] = (image[2, :, :] - mean_z) / std_z

		return image

	def preprocess(self):
		img_ap, img_lat, size_ap, size_lat = self.read_images()
		img = self.concatenate(img_ap, img_lat)
		img = self.toTensor(img)
		img = self.normalize(img, resnet_norm = False)

		return img, size_ap, size_lat
