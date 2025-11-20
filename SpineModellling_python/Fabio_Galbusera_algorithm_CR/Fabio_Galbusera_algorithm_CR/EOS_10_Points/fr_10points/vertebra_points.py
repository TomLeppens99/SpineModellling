import numpy as np

class vertebra_points():
    def __init__(self):
        self.name = ''
        self.points = np.zeros((12, 3), dtype=float)

    def get_point_id(self, pname):
        points = ['Plat_Sup_G', 'Plat_Sup_D', 'Plat_Sup_Ant', 'Plat_Sup_Post',
                  'Plat_Inf_G', 'Plat_Inf_D', 'Plat_Inf_Ant', 'Plat_Inf_Post',
                  'Centroid_G', 'Centroid_D',
                  'Hip_G', 'Hip_D']
        return points.index(pname)

    def set_point(self, pname, px, py, pz):
        p_id = self.get_point_id(pname)
        self.points[p_id, 0] = px
        self.points[p_id, 1] = py
        self.points[p_id, 2] = pz
        return

    def get_point_coords(self, pname):
        p_id = self.get_point_id(pname)
        return self.points[p_id, 0], self.points[p_id, 1], self.points[p_id, 2]

    def update_point_coords(self, view, pname, px, py):
        p_id = self.get_point_id(pname)
        if view == 0:
            self.points[p_id, 1] = px
            self.points[p_id, 2] = py
        else:
            self.points[p_id, 0] = px
            self.points[p_id, 2] = py
        return
    
    def get_crop_region(self, factor, size_x, size_y, size_z, padding):
        print(self.points)
        size_x += padding * 2
        size_y += padding * 2
        size_z += padding * 2

        if (self.name != 'Hips') and (self.name != 'S1'):
            limit_low = 0
            limit_up = 9
        if self.name == 'Hips':
            limit_low = 10
            limit_up = 11
        if self.name == 'S1':
            limit_low = 2
            limit_up = 3

        minx = 1.E8
        maxx = -1.E8
        miny = 1.E8
        maxy = -1.E8
        minz = 1.E8
        maxz = -1.E8
        cx = 0.
        cy = 0.
        cz = 0.
        for ipoint in range(limit_low, limit_up+1):
            if self.points[ipoint, 0] < minx:
                minx = self.points[ipoint, 0]
            if self.points[ipoint, 0] > maxx:
                maxx = self.points[ipoint, 0]
            if self.points[ipoint, 1] < miny:
                miny = self.points[ipoint, 1]
            if self.points[ipoint, 1] > maxy:
                maxy = self.points[ipoint, 1]
            if self.points[ipoint, 2] < minz:
                minz = self.points[ipoint, 2]
            if self.points[ipoint, 2] > maxz:
                maxz = self.points[ipoint, 2]
            cx += float(self.points[ipoint, 0])
            cy += float(self.points[ipoint, 1])
            cz += float(self.points[ipoint, 2])
        cx /= float(limit_up - limit_low) + 1.
        cy /= float(limit_up - limit_low) + 1.
        cz /= float(limit_up - limit_low) + 1.

        found = 0
        while found == 0:
            size_box = int(max([float(maxx)-cx, float(maxy)-cy, float(maxz)-cz, float(cx)-minx, float(cy)-miny, float(cz)-minz])*factor)

            if size_box < 10:
                size_box = 10

            box_min_x = int(cx) - size_box 
            box_max_x = int(cx) + size_box 
            box_min_y = int(cy) - size_box 
            box_max_y = int(cy) + size_box 
            box_min_z = int(cz) - size_box 
            box_max_z = int(cz) + size_box 
            if (box_min_x < 0) or (box_max_x >= size_x) or (box_min_y < 0) or (box_max_y >= size_y) or (box_min_z < 0) or (box_max_z >= size_z):
                factor *= 0.995
            else:
                found = 1

            #print("x: {} {}, size {}\n".format(box_min_x, box_max_x, box_max_x - box_min_x))
            #print("y: {} {}, size {}\n".format(box_min_y, box_max_y, box_max_y - box_min_y))
            #print("x: {} {}, size {}\n".format(box_min_z, box_max_z, box_max_z - box_min_z))
    
        return box_min_x, box_max_x, box_min_y, box_max_y, box_min_z, box_max_z, factor

