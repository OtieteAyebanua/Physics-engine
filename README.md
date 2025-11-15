Collision Detection

Algorithm for detecting collison

#Find distance between objects...
    #We can loop through the object
    #Index 0 (object 1) {
        When we land on an object[i];
        We calculate the distance between that object and others on the scean;
        We can now store that vlaue on a list in that object;
        The first loop would be linear and the second loop would be parallel to the first;
        Meaning after passing an index we won't come back cause the distance of the current index and the past indexs has already been stored on the current index;
        for(i = 0; i < list.Count - 1;i++);
            for(j = i; j < list.count; j++);
    } 
#Resolve collision....
    #Determin E(restitution) from the less bouncy object;
    