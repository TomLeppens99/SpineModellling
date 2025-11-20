import cv2 as cv
import numpy as np
import os
import torch

class ReadImages():
	def __init__(self, path='', image_name=''):
		self.path = path
		self.image_name = image_name

	def read_images_bynames(self, n_ap, n_lat):
		image_anteroposterior = cv.imread(n_ap, cv.IMREAD_COLOR)			
		image_lateral = cv.imread(n_lat, cv.IMREAD_COLOR)
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


	def normalize(self, image_tensor):
		return image_tensor.div(255.)

	def preprocess_bynames(self, n_ap, n_lat):
		img_ap, img_lat, size_ap, size_lat = self.read_images_bynames(n_ap, n_lat)
		img = self.concatenate(img_ap, img_lat)
		img = self.toTensor(img)
		img = self.normalize(img)

		return img, size_ap, size_lat
