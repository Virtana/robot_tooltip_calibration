import json, os
import numpy as np
import matplotlib.pyplot as plt
import cv2
import time
from skimage.measure import label, regionprops, find_contours

##THis file  is to be run first, before main.py to generate json files for ground truth data

#sub_path = "TRAIN"
sub_path = "TEST"
#Uncomment the particular line to create json files for test data or train data

def mask_to_bbox(mask):

    h, w = mask.shape
    border = np.zeros((h,w))

    contours = find_contours(mask ,50)
    for contour in contours:
        for c in contour:
            x = int(c[0])
            y = int(c[1])
            border[x][y] = 255
    bboxes = []
    lbl = label(border)
    props = regionprops(lbl)

    for prop in props:
        x1 = prop.bbox[1]
        y1 = prop.bbox[0]
        x2 = prop.bbox[3]
        y2 = prop.bbox[2]

    return x1,y1,x2,y2

def check_for_range(num):
    if (num>3999):
        num = 3999
    if (num<1):
        num= 1
    return num

def check_for_valid(num):
    if (num>3999):
        return False
    if (num<1):
        return False
    return True

##Ideally, from the unity generated images, the invalid coords should be represented as -1 already .

#The function below is needed to remove the area of the image between the marker cap's clip and the body of the marker cap
def remove_blobs(im_mask):

    contours, hierarchy = cv2.findContours(im_mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
    hull = []

    for i in range(len(contours)):
        # creating convex hull object for each contour
        hull.append(cv2.convexHull(contours[i], False))

    drawing = np.zeros((im_mask.shape[0], im_mask.shape[1], 3), np.uint8)

    for i in range(len(contours)):
        color = (255, 0, 0)  # blue - color for convex hull
        cv2.drawContours(drawing, hull, i, color=(255, 255, 255), thickness=cv2.FILLED)
    return drawing



#initialses the folder if it does not exist
if not os.path.exists( os.getcwd() +'/'+ sub_path+'/FileLength' ):
    os.makedirs( os.getcwd() +'/'+ sub_path+'/FileLength' )

#initialses the folder if it does not exist
if not os.path.exists( os.getcwd() +'/'+ sub_path+'/Annotations' ):
    os.makedirs( os.getcwd() +'/'+ sub_path+'/Annotations' )


lines = []
with open(sub_path+'/coords.txt') as f:
    lines = f.readlines()

count = 0
for liner in lines:

    print(count)

    coord = lines[count]
    line = coord.split(",")
    keyp_xtip =  int(float(line[0]))
    keyp_xtip = 4000 - keyp_xtip
    #keyp_xtip = 4000 - keyp_xtip
    keyp_ytip = int(float(line[1]))
    
    keyp_xtip = check_for_range(keyp_xtip)
    keyp_ytip = check_for_range(keyp_ytip)

    y = cv2.imread( sub_path+'/MaskedImages/MaskedImage'+ str(count) +'.jpg', cv2.IMREAD_GRAYSCALE )
    y = remove_blobs(y)

    cv2.imwrite("temporary.jpg",y)
    y = cv2.imread('temporary.jpg' , cv2.IMREAD_GRAYSCALE)
    #TODO , remove the redundancy of creating this temporary file each time and convert to GRAYSCALE more directly

    X1, Y1, X2, Y2 = mask_to_bbox(y)
    X1 = check_for_range(X1)
    Y1 = check_for_range(Y1)
    X2 = check_for_range(X2)
    Y2 = check_for_range(Y2)
    #The lines above check if the pixel coords is in the right range, if not, it inserts a -1 as the pixel value 

    colour_image = cv2.imread( sub_path+'/Images/Image' + str(count) + '.jpg', 1)
    

    result = colour_image.copy()
    image = cv2.cvtColor(colour_image, cv2.COLOR_BGR2HSV)

    mask1 = cv2.inRange(image, (0,50,20), (5,255,255))
    mask2 = cv2.inRange(image, (175,50,20), (180,255,255))

    mask = cv2.bitwise_or(mask1, mask2 )
    result = cv2.bitwise_and( image , image , mask=mask)

    cv2.imwrite(sub_path+ '/FileLength/RedMask'+ str(count) +'.jpg' , mask)
    file_info = os.stat(sub_path +'/FileLength/RedMask'+ str(count) +'.jpg')
    
    colour_image = cv2.circle(colour_image.copy(), (keyp_xtip, keyp_ytip), 5, (255, 0, 0), 10)
    colour_image = cv2.rectangle(colour_image.copy(), (X1, Y1), (X2, Y2), (0, 255, 0), 2)
    cv2.imwrite('Examine/Image' + str(count) + '.jpg', colour_image)
    #Uncomment the lines above to determine if the json files are being produced correctly

    
    valid_coords = True
    valid_coords = valid_coords and check_for_valid(X1)
    valid_coords = valid_coords and check_for_valid(Y1)
    valid_coords = valid_coords and check_for_valid(X2)
    valid_coords = valid_coords and check_for_valid(Y2)
    valid_coords = valid_coords and check_for_valid(keyp_xtip)
    valid_coords = valid_coords and check_for_valid(keyp_ytip)


    if ((file_info.st_size) >189770 and valid_coords ):
        image_info = {"bboxes":[[ X1 , Y1 , X2 , Y2 ]], "keypoints":[[[ keyp_xtip , keyp_ytip ,1],[keyp_xtip,keyp_ytip,1]]]}
    else:
        image_info = {"bboxes":[[ X1 , Y1 , X2 , Y2 ]], "keypoints":[[[ keyp_xtip , keyp_ytip ,0],[keyp_xtip,keyp_ytip,0]]]}

    #The if statement above determines if the json file marks that the tooltip is visible in the image or not.

    json_info = json.dumps(image_info)
    json_File = open(sub_path+'/Annotations/Image' + str(count) + '.json' ,'w')
    json_File.write(json_info)
    json_File.close()

    count += 1
   
