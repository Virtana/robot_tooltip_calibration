import torch, os, json, cv2, numpy as np, matplotlib.pyplot as plt
from torch.utils.data import Dataset, DataLoader
import torchvision
from torchvision.models.detection.rpn import AnchorGenerator
from torchvision.transforms import functional as F
import albumentations as A
import transforms, utils, engine, train
from utils import collate_fn
from engine import train_one_epoch, evaluate
from datasetclass import ClassDataset
from augmentations import train_transform
from training import get_model

def save_image( path, count ,image, bboxes, keypoints):
    fontsize = 18

    keypoints_classes_ids2names = {0: 'Tip', 1: 'Tip'}

    #for bbox in bboxes:
    #    start_point = (bbox[0], bbox[1])
    #    end_point = (bbox[2], bbox[3])
    #    image = cv2.rectangle(image.copy(), start_point, end_point, (0, 255, 0), 2)

    for kps in keypoints:
        for idx, kp in enumerate(kps):
            image = cv2.circle(image.copy(), tuple(kp), 1, (0, 255, 0), 10)
            image = cv2.putText(image.copy(), " " + str(tuple(kp)), tuple(kp),
                                cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 0), 3, cv2.LINE_AA)
            cv2.imwrite( str(path)+ '/PredictedImage'+ str(count) + '.jpg' , image)

        break
        