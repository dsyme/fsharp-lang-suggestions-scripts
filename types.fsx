module Types = 
    open System

    type Comment = 
        { Submitter : string
          Submitted : DateTime
          Content : string }

    type Idea = 
        {   Number : string
            Submitter : string
            Submitted : DateTime
            Title : string
            Text : string
            Votes : int32
            Comments : Comment list
            Status : string } 