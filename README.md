# Enter the Nexus

Enter the nexus is a re-implementation of the WildStar MMO server. It was inspired by, and takes a lot of the work from [NexusForever](https://github.com/NexusForever/NexusForever). 
The goal is to develop a standalone server, and drop in components to replace some parts of NexusForever that need to be refactored, but cannot easily be refactored.
We also want to implement "Mega servers", which is not a current goal of NexusForever. The current plan is to provide a
kubernetes operator that will dynamically expand and contract the world server processes as necessary.

This is very much in early development, and only has some basic packet parsing code at this time.
