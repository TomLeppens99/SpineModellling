import vertebra_points
import cv2
import numpy as np

class spine_points():
    def __init__(self, fn, padding):
        self.filename = fn
        self.vertebrae = []
        for i in range(19):
            self.vertebrae.append(vertebra_points.vertebra_points())
        self.padding = float(padding)
        self.vertebrae_modified = np.zeros(19, dtype = int)
        self.read()
        return

    def render_image_nolines(self, fname, fname_c, fname_l):
        img_c = cv2.imread(fname_c, cv2.IMREAD_COLOR)
        img_l = cv2.imread(fname_l, cv2.IMREAD_COLOR)
        img_tot = np.concatenate((img_c, img_l), axis = 1)
        cv2.imwrite(fname, img_tot)
        return

    def render_image(self, fname, fname_c, fname_l):
        def remove_padding(px, py, pz):
            px -= self.padding
            py -= self.padding
            pz -= self.padding
            return px, py, pz

        img_c = cv2.imread(fname_c, cv2.IMREAD_COLOR)
        img_l = cv2.imread(fname_l, cv2.IMREAD_COLOR)
        
        for v in self.vertebrae:
            line_color = (255, 0, 0)
            if self.vertebrae_modified[self.get_vertebra_id(v.name)] == 1:
                line_color = (0, 255, 255)

            if v.name == 'Hips':
                px, py, pz = v.get_point_coords('Hip_G')
                px, py, pz = remove_padding(px, py, pz)
                img_c = cv2.circle(img_c, (int(py), int(pz)), 15, (0, 0, 255), 3)
                img_l = cv2.circle(img_l, (int(px), int(pz)), 15, (0, 0, 255), 3)

                px, py, pz = v.get_point_coords('Hip_D')
                px, py, pz = remove_padding(px, py, pz)
                img_c = cv2.circle(img_c, (int(py), int(pz)), 15, (0, 0, 255), 3)
                img_l = cv2.circle(img_l, (int(px), int(pz)), 15, (0, 0, 255), 3)

            elif v.name == 'S1':
                px1, py1, pz1 = v.get_point_coords('Plat_Sup_Ant')
                px1, py1, pz1 = remove_padding(px1, py1, pz1)
                img_c = cv2.circle(img_c, (int(py1), int(pz1)), 15, (0, 0, 255), 3)
                img_l = cv2.circle(img_l, (int(px1), int(pz1)), 15, (0, 0, 255), 3)

                px2, py2, pz2 = v.get_point_coords('Plat_Sup_Post')
                px2, py2, pz2 = remove_padding(px2, py2, pz2)
                img_c = cv2.circle(img_c, (int(py2), int(pz2)), 15, (0, 0, 255), 3)
                img_l = cv2.circle(img_l, (int(px2), int(pz2)), 15, (0, 0, 255), 3)

                img_c = cv2.line(img_c, (int(py1), int(pz1)), (int(py2), int(pz2)), line_color, 2)
                img_l = cv2.line(img_l, (int(px1), int(pz1)), (int(px2), int(pz2)), line_color, 2)

            else:
                px1, py1, pz1 = v.get_point_coords('Plat_Sup_Ant')
                px1, py1, pz1 = remove_padding(px1, py1, pz1)
                img_l = cv2.circle(img_l, (int(px1), int(pz1)), 15, (0, 0, 255), 3)

                px2, py2, pz2 = v.get_point_coords('Plat_Sup_Post')
                px2, py2, pz2 = remove_padding(px2, py2, pz2)
                img_l = cv2.circle(img_l, (int(px2), int(pz2)), 15, (0, 0, 255), 3)

                px3, py3, pz3 = v.get_point_coords('Plat_Inf_Ant')
                px3, py3, pz3 = remove_padding(px3, py3, pz3)
                img_l = cv2.circle(img_l, (int(px3), int(pz3)), 15, (0, 0, 255), 3)

                px4, py4, pz4 = v.get_point_coords('Plat_Inf_Post')
                px4, py4, pz4 = remove_padding(px4, py4, pz4)
                img_l = cv2.circle(img_l, (int(px4), int(pz4)), 15, (0, 0, 255), 3)

                img_l = cv2.line(img_l, (int(px1), int(pz1)), (int(px2), int(pz2)), line_color, 2)
                img_l = cv2.line(img_l, (int(px2), int(pz2)), (int(px4), int(pz4)), line_color, 2)
                img_l = cv2.line(img_l, (int(px1), int(pz1)), (int(px3), int(pz3)), line_color, 2)
                img_l = cv2.line(img_l, (int(px3), int(pz3)), (int(px4), int(pz4)), line_color, 2)

                px1, py1, pz1 = v.get_point_coords('Plat_Sup_G')
                px1, py1, pz1 = remove_padding(px1, py1, pz1)
                img_c = cv2.circle(img_c, (int(py1), int(pz1)), 15, (0, 0, 255), 3)

                px2, py2, pz2 = v.get_point_coords('Plat_Sup_D')
                px2, py2, pz2 = remove_padding(px2, py2, pz2)
                img_c = cv2.circle(img_c, (int(py2), int(pz2)), 15, (0, 0, 255), 3)

                px3, py3, pz3 = v.get_point_coords('Plat_Inf_G')
                px3, py3, pz3 = remove_padding(px3, py3, pz3)
                img_c = cv2.circle(img_c, (int(py3), int(pz3)), 15, (0, 0, 255), 3)

                px4, py4, pz4 = v.get_point_coords('Plat_Inf_D')
                px4, py4, pz4 = remove_padding(px4, py4, pz4)
                img_c = cv2.circle(img_c, (int(py4), int(pz4)), 15, (0, 0, 255), 3)

                img_c = cv2.line(img_c, (int(py1), int(pz1)), (int(py2), int(pz2)), line_color, 2)
                img_c = cv2.line(img_c, (int(py2), int(pz2)), (int(py4), int(pz4)), line_color, 2)
                img_c = cv2.line(img_c, (int(py1), int(pz1)), (int(py3), int(pz3)), line_color, 2)
                img_c = cv2.line(img_c, (int(py3), int(pz3)), (int(py4), int(pz4)), line_color, 2)

                px1, py1, pz1 = v.get_point_coords('Centroid_G')
                px1, py1, pz1 = remove_padding(px1, py1, pz1)

                img_c = cv2.circle(img_c, (int(py1), int(pz1)), 5, (0, 255, 0), 2)
                img_l = cv2.circle(img_l, (int(px1), int(pz1)), 5, (0, 255, 0), 2)

                px1, py1, pz1 = v.get_point_coords('Centroid_D')
                px1, py1, pz1 = remove_padding(px1, py1, pz1)

                img_c = cv2.circle(img_c, (int(py1), int(pz1)), 5, (0, 255, 0), 2)
                img_l = cv2.circle(img_l, (int(px1), int(pz1)), 5, (0, 255, 0), 2)

                px1, py1, pz1 = v.get_point_coords('Spinous-Process')
                px1, py1, pz1 = remove_padding(px1, py1, pz1)

                img_c = cv2.circle(img_c, (int(py1), int(pz1)), 5, (0, 255, 0), 2)
                img_l = cv2.circle(img_l, (int(px1), int(pz1)), 5, (0, 255, 0), 2)

        img_tot = np.concatenate((img_c, img_l), axis = 1)
        cv2.imwrite(fname, img_tot)
        return

    def write_skipped(self, fname):
        f_out = open(fname, 'w')
        f_out.write("skipped\n")
        f_out.close()
        return

    def write_to_file(self, fname):
        def remove_padding(px, py, pz):
            px -= self.padding
            py -= self.padding
            pz -= self.padding
            return px, py, pz

## edited D.L (added Spinous_Process)
        names = ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9', 'T10', 'T11', 'T12', 'L1', 'L2', 'L3', 'L4', 'L5', 'S1', 'Hips']
        points = ['Plat_Sup_G', 'Plat_Sup_D', 'Plat_Sup_Ant', 'Plat_Sup_Post',
                  'Plat_Inf_G', 'Plat_Inf_D', 'Plat_Inf_Ant', 'Plat_Inf_Post',
                  'Centroid_G', 'Centroid_D', 'Spinous_Process']
        f_out = open(fname, 'w')
        f_out.write("X: anteroposterior coordinate (lat image); Y: laterolateral coordinate (ap image); Z: craniocaudal coordinate (shared)")
        for vname in names:
            v_id = self.get_vertebra_id(vname)
            v = self.vertebrae[v_id]
            if (vname != 'Hips') and (vname != 'S1'):
                for pname in points:
                    px, py, pz = v.get_point_coords(pname)
                    px, py, pz = remove_padding(px, py, pz)
                    f_out.write("{},{},{},{},{}\n".format(vname,pname,px,py,pz))
            if vname == 'S1':
                px, py, pz = v.get_point_coords('Plat_Sup_Ant')
                px, py, pz = remove_padding(px, py, pz)
                f_out.write("S1,Plat_Sup_Ant,{},{},{}\n".format(px, py, pz))
                px, py, pz = v.get_point_coords('Plat_Sup_Post')
                px, py, pz = remove_padding(px, py, pz)
                f_out.write("S1,Plat_Sup_Post,{},{},{}\n".format(px, py, pz))
            if vname == 'Hips':
                px, py, pz = v.get_point_coords('Hip_G')
                px, py, pz = remove_padding(px, py, pz)
                f_out.write("Hips,Hip_G,{},{},{}\n".format(px, py, pz))
                px, py, pz = v.get_point_coords('Hip_D')
                px, py, pz = remove_padding(px, py, pz)
                f_out.write("Hips,Hip_D,{},{},{}\n".format(px, py, pz))

        f_out.close()
        return

    def get_vertebra_id(self, vname):
        names = ['L5', 'L4', 'L3', 'L2', 'L1', 'T12', 'T11', 'T10', 'T9', 'T8', 'T7', 'T6', 'T5', 'T4', 'T3', 'T2', 'T1', 'S1', 'Hips']
        return names.index(vname)

    def get_lower_vertebra_id(self, vname):
        if vname == 'S1':
            return self.get_vertebra_id('Hips')
        if vname == 'L5':
            return self.get_vertebra_id('S1')
        if vname == 'L4':
            return self.get_vertebra_id('L5')
        if vname == 'L3':
            return self.get_vertebra_id('L4')
        if vname == 'L2':
            return self.get_vertebra_id('L3')
        if vname == 'L1':
            return self.get_vertebra_id('L2')
        if vname == 'T12':
            return self.get_vertebra_id('L1')
        if vname == 'T11':
            return self.get_vertebra_id('T12')
        if vname == 'T10':
            return self.get_vertebra_id('T11')
        if vname == 'T9':
            return self.get_vertebra_id('T10')
        if vname == 'T8':
            return self.get_vertebra_id('T9')
        if vname == 'T7':
            return self.get_vertebra_id('T8')
        if vname == 'T6':
            return self.get_vertebra_id('T7')
        if vname == 'T5':
            return self.get_vertebra_id('T6')
        if vname == 'T4':
            return self.get_vertebra_id('T5')
        if vname == 'T3':
            return self.get_vertebra_id('T4')
        if vname == 'T2':
            return self.get_vertebra_id('T3')
        if vname == 'T1':
            return self.get_vertebra_id('T2')

    def get_upper_vertebra_id(self, vname):
        if vname == 'Hips':
            return self.get_vertebra_id('S1')
        if vname == 'S1':
            return self.get_vertebra_id('L5')
        if vname == 'L5':
            return self.get_vertebra_id('L4')
        if vname == 'L4':
            return self.get_vertebra_id('L3')
        if vname == 'L3':
            return self.get_vertebra_id('L2')
        if vname == 'L2':
            return self.get_vertebra_id('L1')
        if vname == 'L1':
            return self.get_vertebra_id('T12')
        if vname == 'T12':
            return self.get_vertebra_id('T11')
        if vname == 'T11':
            return self.get_vertebra_id('T10')
        if vname == 'T10':
            return self.get_vertebra_id('T9')
        if vname == 'T9':
            return self.get_vertebra_id('T8')
        if vname == 'T8':
            return self.get_vertebra_id('T7')
        if vname == 'T7':
            return self.get_vertebra_id('T6')
        if vname == 'T6':
            return self.get_vertebra_id('T5')
        if vname == 'T5':
            return self.get_vertebra_id('T4')
        if vname == 'T4':
            return self.get_vertebra_id('T3')
        if vname == 'T3':
            return self.get_vertebra_id('T2')
        if vname == 'T2':
            return self.get_vertebra_id('T1')

    def add_point(self, v, p, px, py, pz):
        v_id = self.get_vertebra_id(v)
        self.vertebrae[v_id].set_point(p, px, py, pz)
        self.vertebrae[v_id].name = v
        return
## edited D.L (changed 10 to 11 for T1-L5 & added: lines_points.append('Spinous_Process'))
    def read(self):
        lines_vertebrae = []
        for i in range(11):
            lines_vertebrae.append('T1')
        for i in range(11):
            lines_vertebrae.append('T2')
        for i in range(11):
            lines_vertebrae.append('T3')
        for i in range(11):
            lines_vertebrae.append('T4')
        for i in range(11):
            lines_vertebrae.append('T5')
        for i in range(11):
            lines_vertebrae.append('T6')
        for i in range(11):
            lines_vertebrae.append('T7')
        for i in range(11):
            lines_vertebrae.append('T8')
        for i in range(11):
            lines_vertebrae.append('T9')
        for i in range(11):
            lines_vertebrae.append('T10')
        for i in range(11):
            lines_vertebrae.append('T11')
        for i in range(11):
            lines_vertebrae.append('T12')
        for i in range(11):
            lines_vertebrae.append('L1')
        for i in range(11):
            lines_vertebrae.append('L2')
        for i in range(11):
            lines_vertebrae.append('L3')
        for i in range(11):
            lines_vertebrae.append('L4')
        for i in range(11):
            lines_vertebrae.append('L5')
        for i in range(2):
            lines_vertebrae.append('S1')
        for i in range(2):
            lines_vertebrae.append('Hips')

        lines_points = []
        for i in range(17):
            lines_points.append('Plat_Sup_G')
            lines_points.append('Plat_Sup_D')
            lines_points.append('Plat_Sup_Ant')
            lines_points.append('Plat_Sup_Post')
            lines_points.append('Plat_Inf_G')
            lines_points.append('Plat_Inf_D')
            lines_points.append('Plat_Inf_Ant')
            lines_points.append('Plat_Inf_Post')
            lines_points.append('Centroid_G')
            lines_points.append('Centroid_D')
            lines_points.append('Spinous_Process')
        lines_points.append('Plat_Sup_Ant')
        lines_points.append('Plat_Sup_Post')
        lines_points.append('Hip_G')
        lines_points.append('Hip_D')            

        with open(self.filename, 'r') as f:
            lines = f.readlines()

        for iline in range(len(lines)):
            current_vertebra = lines_vertebrae[iline]
            current_point = lines_points[iline]

            line = lines[iline].split(',')
            print("v {}, point {} -> {} {} {}\n".format(current_vertebra, current_point, float(line[2]), float(line[3]), float(line[4])))
            self.add_point(current_vertebra, current_point, float(line[2]) + self.padding, float(line[3]) + self.padding, float(line[4]) + self.padding)

        return

    def get_crop_region(self, vname, factor, size_x, size_y, size_z):
        v_id = self.get_vertebra_id(vname)
        return self.vertebrae[v_id].get_crop_region(factor, size_x, size_y, size_z, self.padding)

