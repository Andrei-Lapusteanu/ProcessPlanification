# ProcessPlanification

## Summary

 Process Planification is an application used to simulate the scheduling of processes inside a CPU
 
<a href="https://imgur.com/m6Pjvqw"><img src="https://i.imgur.com/m6Pjvqw.png" title="source: imgur.com" /></a>

# Technologies
- Written exclusively in **C#**
- Its backbone is a slightly modified **MVC (Model-View-Controller)** design pattern, to which an **Entities** project is added
- UI designed in **WPF/XAML**

# Functionality

- Simulation of 4 different process scheduling algorithms: 
	- First Come First Serve
	- Shortest Job Next
	- Priority Scheduling
	- Round Robin
- Simulate using **1 to 4 virtual CPUs**
- Create, define, randomize, load, and save input data
- **Increment and decrement time** in order to view the placement of processes inside a CPU

## Important
I do not guarantee an entirely accurate functioning of all of the aforementioned algorithms **for multiple CPUs and for Round Robin scheduling**. As this was a project for school, being constrained by time I did not manage to properly verify all functionalities

<!--stackedit_data:
eyJoaXN0b3J5IjpbLTc0MzU1NDQ5MCwxNDg0MDcxMzk2XX0=
-->