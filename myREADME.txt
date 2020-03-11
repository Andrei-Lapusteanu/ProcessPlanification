                  ___  ___  ____  _________________               
                 / _ \/ _ \/ __ \/ ___/ __/ __/ __/               
                / ___/ , _/ /_/ / /__/ _/_\ \_\ \                 
               /_/ _/_/|_|\____/\___/___/___/___/
   ___  __   ___   _  ______________________ ______________  _  __
  / _ \/ /  / _ | / |/ /  _/ __/  _/ ___/ _ /_  __/  _/ __ \/ |/ /
 / ___/ /__/ __ |/    // // _/_/ // /__/ __ |/ / _/ // /_/ /    / 
/_/  /____/_/ |_/_/|_/___/_/ /___/\___/_/ |_/_/ /___/\____/_/|_/ 

 ________________________________
| ======== Ver. 1.0.0.1 ======== |
| ============================== |
| ====== Andrei Lapusteanu ===== |
| ========= June 2018  ========= |
|________________________________|

=====================
I. TECHNICAL DETAILS:

Built on: Window 10 Pro 
	  Version 1709
	  OS build 16299.431
	  64-bit operating system, x64-based processor

IDE version: Microsoft Visual Studio Enterprise 2015
             Version 14.0.24720.00 Update 1
		
Project properties: Debug and Release configurations are built for "Any CPU"
		    Startup project: "View"
		    No 3rd party libraries or NuGet packages were used
		    Built for .NET Framework 4.0.0
			
========================
II. QUICK START TUTORIAL

1.   Disable your antivirus to stop it from checking the executable

2.   Start up "ProcessPlanification.sln" from the project directory if you have Visual Studio installed and if you want to view or debug the code
     Click on "ProcessPlanification_DEBUG_1.0.0.1.exe" or "ProcessPlanification_RELEASE_1.0.0.1.exe" shortcuts
     If not, try the Debug version from <path to project>\ProcessPlanification\ProcessPlanification\bin\Debug\ProcessPlanification.exe
     If not, try the Release version from <path to project>\ProcessPlanification\ProcessPlanification\bin\Release\ProcessPlanification.exe

3.   Click on "Edit & Load Data" in the bottom-right corner of the screen

4.1. If you want to see the application in action with some preset data, click on "Load" in the center-top on the new window and open "process.txt"
4.2. If you want to enter your data, click on "Enable Editing" on the bottom-right corner of the screen

5.   You can Edit any data by pressing "Enable Editing", use the plus and minus signs to add or delete rows, and select with the mouse a cell and enter or edit data
     After editing, make sure to click on "Finish Editing". Check the status message on the bottom of the window to see if data is valid (or not)

6.   With valid data, click Submit to return to the main window

7.   On the main window, the original data set table should now be populated

8.   In the bottom-right of the screen, select values for the "CPU Cores" and "Algorithm" from the drop-down lists

9.1. If you want to see the completed algorithm, click on the pretty green button "Generate & Run"
9.2. If you want to generate the data at time = 0, click on "Generate"

10.  You can generate as many of these windows as you want. Also, you can delete all by clicking "Clear" or only the ones you do not want by clicking on the "X" in the right side of each control's title bar. If you do not want to see them, you can minimize them by clicking on the arrow on the left of the "X" button in the title bar

11.  IMPORTANT: If you want to use the features "Increment" and "Decrement" time as well as to see the table "Data Set at time = <number>" you must click on the title bar of the generated control (the one generated at 9.1 or 9.2) in order to SELECT it. To deselect, click again. The control should highlight if selected

12.  With the mouse over the content of the generated controls, you can zoom in or out by using the mouse wheel and also pan by left-clicking and dragging

Other helpful tips can be found on the UI

CAUTION: I cannot guarantee the validity of the algorithms for all input data. Use Round Robin for +1 CPUs with caution, it is yet to be optimised

=============
III. FEATURES

Process Planification is a program used to simulate the scheduling of processes inside a CPU

- Simulation of 4 different process scheduling algorithms: - First Come First Serve
						           - Shortest Job Next
							   - Priority Scheduling
							   - Round Robin
- Simulate on 1, 2, 3 or 4 virtual CPUs
- Create, define, randomize, load, and save input data 
- Generate empty pipelines as well as completed pipelines (from the input data)
- Generate, minimize or delete as many different "mini-windows" containing the processed visual data inside the main scrollable area
- Increment and Decrement time in order to view the placement of processes inside a CPU
- View at any given time the original processes information as well as information regarding the current time
- Jump to any time and update UI elements by clicking on the timeline
- Zoom and pan inside the "mini-windows" to fit the content to your desire

====================================
IV. PROJECT DETAILS AND ORGANISATION

- Process Planification is written excusively in C# and its UI in WPF/XAML
- Its backbone is a slightly modified MVC (Model-View-Controller) design pattern, to which an Entities project is added

#####################
- Controller project: This project handles the back-end of the application, more precisely, the processing of the input data with the aforemention 4 algorithm
	
	- FirstComeFirstServe.cs: Implements methods for the First Come First Serve algorithm
	- ShortestJobNext.cs: 	  Implements methods for the Shortest Job Next algorithm
	- PriorityScheduling.cs:  Implements methods for the Priority Scheduling algorithm
	- RoundRobin.cs:	      Implements methods for the Round Robin algorithm
	- SharedFuntions.cs:      Contains a number of methods that are shared between the above classes. It exists as a need to remove redundancy, because there are some methods which are idendtical for each algorithm
	- SortingAlgorithms.cs:   Implements a number of sorting algorithm for the processes lists
	
###################
- Entities project: This project contains the classes used to define the custom objects used in the application. It exists because of a need to better organize the application

	- DataGridContext.cs: Is used to help with the filling of grids inside "EditLoadData.xaml.cs" (see View project for details)
	- DeadTime.cs:        Represents dead time inside the pipeline
 	- Process.cs          Main data type used. It encapsulates important fields, such as: 
			      		- private int id;
					- private int arrivalTime;
					- private int proccesingTime;
					- private int priority;
			      		----- these are the fields populated when valid data in loaded in the application
	- WindowClosingEventType.cs: Helps with the Window_Closing event within "EditLoadData.xaml.cs" (see View for details)
	
################
- Model project: This Project contains classes used for Input/Output
	
	- FileIO.cs: Implements methods used to load and save files, such as ReadFromFile(string filePath) and SaveToFile(List<Process> processList, string filePath)
	
###############
- View Project: This is startup project, which contains the UI and several methods used to correctly display data on screen

	- MainWindow.xaml: This is the main window. All controls and other windows can be accessed from here. MainWindow.xaml.cs handles the logic for the window
	- EditLoadData.xaml: This is the window that handles creating, defining, randomizing, loading, and saving input data. Data from here can be submitted to MainWindow.xaml for further processing
	- PipelineControl.xaml: This is a custom User Control which contains the visual representations of the processes inside the CPUs. After generating and running an algorithm, you can minimize or close this "mini-window", or hover with the mouse over the processes to view details
	- TimeScaleElement.xaml: This is a custom User Control which is used to define a tick in the timescale. It can be clicked to view processes at the desired time increment.
	- ZoomBorder.cs: Used to display the processes inside the PipelineControl. It has options to pan & zoom.
	- ZoomBorderOnlyPan.cs: Used in the MainWindow to scroll the main area (in which PipelineControls are genrated), or scroll the Data Sets. Panning is disabled
	
====================================
