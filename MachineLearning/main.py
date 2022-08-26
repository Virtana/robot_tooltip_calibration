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
from visualize import visualize


KEYPOINTS_FOLDER_TRAIN = 'TRAIN'
dataset = ClassDataset(KEYPOINTS_FOLDER_TRAIN, transform= train_transform() , demo=True)
data_loader = DataLoader(dataset, batch_size=1, shuffle=True, collate_fn=collate_fn)

iterator = iter(data_loader)
batch = next(iterator)

#print("Original targets:\n", batch[3], "\n\n")
#print("Transformedd targets:\n", batch[1])

image = (batch[0][0].permute(1, 2, 0).numpy() * 255).astype(np.uint8)
print(batch[1][0]['keypoints'])
bboxes = batch[1][0]['boxes'].detach().cpu().numpy().astype(np.int32).tolist()

keypoints = []
for kps in batch[1][0]['keypoints'].detach().cpu().numpy().astype(np.int32).tolist():
    keypoints.append([kp[:2] for kp in kps])

image_original = (batch[2][0].permute(1, 2, 0).numpy() * 255).astype(np.uint8)
bboxes_original = batch[3][0]['boxes'].detach().cpu().numpy().astype(np.int32).tolist()

keypoints_original = []
for kps in batch[3][0]['keypoints'].detach().cpu().numpy().astype(np.int32).tolist():
    keypoints_original.append([kp[:2] for kp in kps])


visualize(1, image, bboxes, keypoints, image_original, bboxes_original, keypoints_original)

device = torch.device('cuda') if torch.cuda.is_available() else torch.device('cpu')

################################################################################

KEYPOINTS_FOLDER_TRAIN = 'TRAIN'
KEYPOINTS_FOLDER_TEST = 'TEST'

dataset_train = ClassDataset(KEYPOINTS_FOLDER_TRAIN, transform=train_transform(), demo=False)
dataset_test = ClassDataset(KEYPOINTS_FOLDER_TEST, transform=None, demo=False)

data_loader_train = DataLoader(dataset_train, batch_size=2, shuffle=True, collate_fn=collate_fn)
data_loader_test = DataLoader(dataset_test, batch_size=1, shuffle=False, collate_fn=collate_fn)

model = get_model(num_keypoints=2)
model.to(device)

params = [p for p in model.parameters() if p.requires_grad]
optimizer = torch.optim.SGD(params, lr=0.001, momentum=0.9, weight_decay=0.0005)
lr_scheduler = torch.optim.lr_scheduler.StepLR(optimizer, step_size=5, gamma=0.3)
num_epochs = 3

for epoch in range(num_epochs):
    train_one_epoch(model, optimizer, data_loader_train, device, epoch, print_freq=1000)
    lr_scheduler.step()
    #evaluate(model, data_loader_test, device)
    #The built in evaluation function is disabled to evaluate instead with test_many_img and the manually created evaluate file
    print(" Epoch" + str(epoch))
# Save model weights after training
torch.save(model.state_dict(), 'keypointsrcnn_weights.pth')
