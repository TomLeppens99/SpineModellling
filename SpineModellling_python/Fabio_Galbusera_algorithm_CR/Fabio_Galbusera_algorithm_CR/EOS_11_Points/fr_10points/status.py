class status():
    def __init__(self):
        self.vertebrae = ['Hips', 'S1', 'L5', 'L4', 'L3', 'L2', 'L1', 'T12', 'T11', 'T10', 'T9', 'T8', 'T7', 'T6', 'T5', 'T4', 'T3', 'T2', 'T1']
        self.current_vertebra = 0
        self.regions = ['l_ep', 'u_ep', 'ped', 'proc'] # Edited D.L (added 'proc')
        self.current_region = 0

    def next(self): 
        new_vertebra = False
        last_region = False
        if (self.vertebrae[self.current_vertebra] == 'Hips') or (self.vertebrae[self.current_vertebra] == 'S1'):
            self.current_vertebra += 1
            new_vertebra = True
            self.current_region = 0
        else:
            self.current_region += 1
            if (self.current_region == 3) and (self.current_vertebra == 18): # Edited D.L (from 2 to 3)
                last_region = True
            elif self.current_region == 4: # Edited D.L (from 3 to 4)
                self.current_region = 0
                self.current_vertebra += 1
                new_vertebra = True
                    
        return new_vertebra, last_region

    def skip_spine(self):
        self.current_vertebra = 0
        self.current_region = 0
        return

    def previous(self): 
        new_vertebra = False
        if self.vertebrae[self.current_vertebra] == 'S1':
            self.current_vertebra = 0
            self.current_region = 0
            new_vertebra = True
        elif self.vertebrae[self.current_vertebra] == 'Hips':
            self.current_vertebra = 0
            self.current_region = 0
        elif (self.vertebrae[self.current_vertebra] == 'L5') and (self.regions[self.current_region] == 'l_ep'):
            self.current_vertebra -= 1
            new_vertebra = True
            self.current_region = 0
        else:
            self.current_region -= 1
            if self.current_region < 0:
                self.current_region = 2
                self.current_vertebra -= 1
                new_vertebra = True
        return new_vertebra

    def get_vertebra(self):
        return self.vertebrae[self.current_vertebra]

    def get_region(self):
        return self.regions[self.current_region]