import cv2 as cv
import numpy as np
import torch
import os
import pandas as pd

class ReadImages():
    def __init__(self, path, image_name):
        self.path = path
        self.image_name = image_name

    def _read_or_none(self, fname):
        return cv.imread(os.path.join(self.path, fname), cv.IMREAD_COLOR)

    def read_images(self):
        # AP
        image_anteroposterior = self._read_or_none(self.image_name)

        # LAT: try *_S.* then *_L.*
        base, ext = os.path.splitext(os.path.basename(self.image_name))
        lat_S = base[:-1] + 'S' + ext
        lat_L = base[:-1] + 'L' + ext

        image_lateral = self._read_or_none(lat_S)
        if image_lateral is None:
            image_lateral = self._read_or_none(lat_L)

        # Fail early with explicit message
        missing = []
        if image_anteroposterior is None:
            missing.append(self.image_name)
        if image_lateral is None:
            missing.append(lat_S + " (or " + lat_L + ")")

        if missing:
            raise FileNotFoundError(
                "Could not read required image(s): " + ", ".join(missing)
            )

        size_ap  = image_anteroposterior.shape[:2]
        size_lat = image_lateral.shape[:2]

        return self.resize(image_anteroposterior), self.resize(image_lateral), size_ap, size_lat

    def concatenate(self, image_anteroposterior, image_lateral):
        return np.concatenate((image_anteroposterior, image_lateral), axis=1)

    def resize(self, image, size=(1024, 512)):
        if image is None:
            raise ValueError("resize() received None image")
        h, w = image.shape[:2]
        new_h, new_w = int(size[0]), int(size[1])
        return cv.resize(image, dsize=(new_w, new_h), interpolation=cv.INTER_CUBIC)

    def toTensor(self, image_concatenated):
        return torch.from_numpy(image_concatenated).permute(2, 0, 1).float()

    def normalize(self, image, resnet_norm):
        image = image.div(255.)
        if resnet_norm:
            mean_x, mean_y, mean_z = 0.485, 0.456, 0.406
            std_x,  std_y,  std_z  = 0.229, 0.224, 0.225
            image[0, :, :] = (image[0, :, :] - mean_x) / std_x
            image[1, :, :] = (image[1, :, :] - mean_y) / std_y
            image[2, :, :] = (image[2, :, :] - mean_z) / std_z
        return image

    def preprocess(self):
        img_ap, img_lat, size_ap, size_lat = self.read_images()
        img = self.concatenate(img_ap, img_lat)
        img = self.toTensor(img)
        img = self.normalize(img, resnet_norm=False)
        return img, size_ap, size_lat
