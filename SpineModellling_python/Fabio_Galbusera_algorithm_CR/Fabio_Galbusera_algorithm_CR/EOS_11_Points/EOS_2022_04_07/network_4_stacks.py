import hourglass
import torch
import torch.nn as nn
import dsntnn

class CoordRegressionNetwork(torch.nn.Module):
    def __init__(self):
        super().__init__()
        self.fcn = self.backbone_model()        
        
    def forward(self, images):
        #use 1x1 conv to have 1 heatmap for each location
        unnormalized_heatmaps = self.fcn(images)
        #Normalize heatmap
        heatmaps = []
        coords = []
        for i in range(len(unnormalized_heatmaps)):
            hm = dsntnn.flat_softmax(unnormalized_heatmaps[i])
            heatmaps.append(hm)
            #coordinates calculation
            c = dsntnn.dsnt(hm)
            coords.append(c)

        return coords, heatmaps

    def backbone_model(self):
        #hg_model = hourglass.HourglassNet(hourglass.Bottleneck, num_stacks = 4, num_blocks = 2, num_classes = 756)
        hg_model = hourglass.HourglassNet(hourglass.Bottleneck, num_stacks = 2, num_blocks = 2, num_classes = 348)
        return hg_model