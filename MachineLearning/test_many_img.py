import torch, os, json, cv2, numpy as np, matplotlib.pyplot as plt
from torch.utils.data import Dataset, DataLoader
import torchvision
from torchvision.models.detection.rpn import AnchorGenerator
from torchvision.transforms import functional as F
import albumentations as A
import transforms, utils, engine, train
from utils import collate_fn
from engine import train_one_epoch, evaluate
from datasetclass import ClassDataset, PredDataset
from augmentations import train_transform
from training import get_model
from visualize import visualize
from save_image import save_image

predicted_coords = []
truth_coords = []

#initialses the folder of PRedicted images if it does not exist
if not os.path.exists( os.getcwd() +'/PredictedImages' ):
    os.makedirs( os.getcwd() +'/PredictedImages' )

device = torch.device('cuda') if torch.cuda.is_available() else torch.device('cpu')


model = get_model(num_keypoints = 2, weights_path = 'keypointsrcnn_weights.pth')
model.to(device)

KEYPOINTS_FOLDER_TEST = 'TEST'
dataset_test = ClassDataset(KEYPOINTS_FOLDER_TEST, transform=None, demo=False)
data_loader_test = DataLoader(dataset_test, batch_size=1, shuffle=False, collate_fn=collate_fn)

iterator = iter(data_loader_test)

for idx in range(len(next(os.walk( KEYPOINTS_FOLDER_TEST +"/Images" ))[2])):
    print(idx)
    images, targets = next(iterator)
    batch = images
    images = list(image.to(device) for image in images)

    with torch.no_grad():
        model.to(device)
        model.eval()
        output = model(images)

    image = (images[0].permute(1, 2, 0).detach().cpu().numpy() * 255).astype(np.uint8)
    scores = output[0]['scores'].detach().cpu().numpy()

    high_scores_idxs = np.where(scores > 0.7)[0].tolist()  # Indexes of boxes with scores > 0.7
    post_nms_idxs = torchvision.ops.nms(output[0]['boxes'][high_scores_idxs], output[0]['scores'][high_scores_idxs],
                                        0.3).cpu().numpy()  # Indexes of boxes left after applying NMS (iou_threshold=0.3)

    ################################################################################################################

    keypoints_g_truth = []
    for kps in targets[0]['keypoints'].detach().cpu().numpy().astype(np.int32).tolist():
        keypoints_g_truth.append([kp[:2] for kp in kps])
        break

    ################################################################################################################
    keypoints = []
    for kps in output[0]['keypoints'][high_scores_idxs][post_nms_idxs].detach().cpu().numpy():
        keypoints.append([list(map(int, kp[:2])) for kp in kps])
        break

    bboxes = []
    for bbox in output[0]['boxes'][high_scores_idxs][post_nms_idxs].detach().cpu().numpy():
        bboxes.append(list(map(int, bbox.tolist())))

    ################################################################################################################
    #visualize(idx  , image, bboxes, keypoints)
    save_image('PredictedImages', idx  , image, bboxes, keypoints)

    if (len(keypoints)):
        pixel_coord = str(keypoints[0][0])
    else:
        pixel_coord = " -1, -1 "
    predicted_coords.append( pixel_coord[1:-1] )

    pixel_coord = str(keypoints_g_truth[0][0])
    truth_coords.append( pixel_coord[1:-1] )

#################################################################

#The following are used for manual evaluation

if os.path.isfile('PredictedImages/predicted_coords.txt'):
  os.remove('PredictedImages/predicted_coords.txt')

with open('PredictedImages/predicted_coords.txt', 'w') as f:
    f.write('\n'.join(predicted_coords))


if os.path.isfile('PredictedImages/truth_coords.txt'):
  os.remove('PredictedImages/truth_coords.txt')

with open('PredictedImages/truth_coords.txt', 'w') as f:
    f.write('\n'.join(truth_coords))