import math,statistics


##Need to run test_many_img first

g_truth_coords = []
with open('truth_coords.txt') as f:
    g_truth_coords = f.readlines()

predicted_coords = []
with open('predicted_coords.txt') as f:
    predicted_coords = f.readlines()

list_x =[]
list_y =[]
list_dist = []


count = 0
for idx in range(len(predicted_coords)):

    g_truth_coord = g_truth_coords[count]
    predicted_coord = predicted_coords[count]

    seperated_g_truth = g_truth_coord.split(",")
    seperated_predicted = predicted_coord.split(",")

    if not( (float(seperated_predicted[0])) == -1 ):

        #Calculate the error between the predicted and ground truth in x and y plane and the overall distance
        delta_x = abs(((float(seperated_g_truth[0])))-((float(seperated_predicted[0]))))
        delta_y = abs(((float(seperated_g_truth[1]))) - ((float(seperated_predicted[1]))))
        dist = math.sqrt( (delta_x**2) + (delta_y**2) )

        #Uncomment the follwoing line to identify any outliers
        #if ((delta_y > 20) | (delta_x > 20)):
        #    print( "Count  " +str(count) )
        #    print(delta_x)
        #    print(delta_y)
        #    print("\n")

        #Adds each error to a list
        list_x.append(delta_x)
        list_y.append(delta_y)
        list_dist.append(dist)

    count = count + 1

print("Final results \n")

print("Standard deviation of error in X plane is:  "+str(statistics.pstdev(list_x)))
print("Standard deviation of error in Y plane is:  "+str(statistics.pstdev(list_y)))
print("Standard deviation of error in pythagoras straight line distance is:  "+str(statistics.pstdev(list_dist)))

print("\n")

print("Mean of error in X plane is:  "+str(statistics.pstdev(list_x)))
print("Mean of error in Y plane is:  "+str(statistics.pstdev(list_y)))
print("Mean of error in pythagoras straight line distance is:  "+str(statistics.mean(list_dist)))

print("\n")

print("Max of error in X plane is:  "+str(max(list_x)))
print("Max error in Y plane is:  "+str(max(list_y)))
print("Max error in pythagoras straight line distance is:  "+str(max(list_dist)))

print("\n")

