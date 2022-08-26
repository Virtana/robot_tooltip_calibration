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

def visualize( count ,image, bboxes, keypoints, image_original=None, bboxes_original=None, keypoints_original=None):
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
        break

    #
    if image_original is None and keypoints_original is None:
        plt.figure(figsize=(40, 40))
        plt.imshow(image)
        plt.show()
        


    else:
        for bbox in bboxes_original:
            start_point = (bbox[0], bbox[1])
            end_point = (bbox[2], bbox[3])
            image_original = cv2.rectangle(image_original.copy(), start_point, end_point, (0, 255, 0), 2)
        for kps in keypoints_original:
           for idx, kp in enumerate(kps):
                image_original = cv2.circle(image_original, tuple(kp), 5, (255, 0, 0), 10)
                image_original = cv2.putText(image_original, " " + keypoints_classes_ids2names[idx], tuple(kp),
                                             cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 0, 0), 3, cv2.LINE_AA)
        f, ax = plt.subplots(1, 2, figsize=(40, 20))
        ax[0].imshow(image_original)
        ax[0].set_title('Original image', fontsize=fontsize)
        ax[1].imshow(image)
        
        ax[1].set_title('Transformed image', fontsize=fontsize)
        plt.show()
