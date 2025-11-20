import hourglass
import torch
import torch.nn as nn
import dsntnn

class CoordRegressionNetwork(torch.nn.Module):
    def __init__(self, n_locations, in_channels):
        super().__init__()
        self.fcn = self.backbone_model()
        #output DSNT
        self.hm1 = torch.nn.Conv2d(128, n_locations, kernel_size = 1, bias = False)
        self.hm2 = torch.nn.Conv2d(128, n_locations, kernel_size = 1, bias = False)
        
    def forward(self, images):
        fcn_out = self.fcn(images)
        
        heatmaps = []
        coords = []
        for i in range(len(fcn_out)):
            if i == 0:
                unnormalized_heatmap = self.hm1(fcn_out[i])
                hm = dsntnn.flat_softmax(unnormalized_heatmap)
                heatmaps.append(hm)
                #coordinates calculation
                c = dsntnn.dsnt(hm)
                coords.append(c)
            else:
                unnormalized_heatmap = self.hm2(fcn_out[i])
                hm = dsntnn.flat_softmax(unnormalized_heatmap)
                heatmaps.append(hm)
                #coordinates calculation
                c = dsntnn.dsnt(hm)
                coords.append(c)

        return coords, heatmaps

    def backbone_model(self):
	    hg_model = hourglass.HourglassNet(hourglass.Bottleneck, num_stacks = 2, num_blocks = 2, num_classes = 128)
	    return hg_model