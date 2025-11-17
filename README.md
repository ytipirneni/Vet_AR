# Vet AR: Augmented Reality Application for Veterinary Surgical Annotation Simulation

## Overview
This repository contains the Unity-based augmented reality (AR) application developed for a pilot study evaluating the feasibility of AR glasses in veterinary surgical oncology. The application integrates hand-tracking, holographic 3D canine cadaver models, and annotation tools to assess spatial accuracy in surgical planning and training. This work is associated with the manuscript: **Tipirneni Y, Goldschmidt S. Pilot study to assess the utility of augmented reality (AR) glasses for spatial tracking and surgical annotation in veterinary medicine. *Frontiers in Veterinary Science* (Oral and Maxillofacial Surgery Section), 2025.**

## Features
- Interactive 3D visualization of canine cadaver oral anatomy  
- Integration with **XReal Air 2 Ultra** AR glasses and **Beam Pro** console  
- Real-time **hand-tracking** for direct annotations  
- Distance and area accuracy measurement modules  
- 1:1 holographic models generated via iPhone LiDAR scans  

## Repository Contents
- `Assets/` — Unity assets, scripts, prefabs, and materials  
- `Packages/` — Unity package manager dependencies  
- `ProjectSettings/` — Unity project configurations  
- `AR_Unity.zip` — Complete archived Unity project for download  
- `LICENSE` — MIT license (open use with attribution)  
- `README.md` — Project documentation (this file)  

## Requirements
- **Unity Editor**: 2022.3 LTS (or later recommended)  
- **XReal SDK for Unity**  
- **Android XR Plugin** (for hand tracking)  
- Hardware tested: **XReal Beam Pro** + **XReal Air 2 Ultra glasses**  

## Installation & Setup
1. Download the repo or the `AR_Unity.zip` archive.  
2. If using the archive, unzip into your Unity projects directory.  
3. Open the project in **Unity Hub** (ensure Unity 2022.3 LTS or later).  
4. Install required packages via Unity Package Manager (XReal SDK, Android XR plugin).  
5. Build and deploy to Android device connected to XReal glasses.  

## Usage
1. Launch the AR application on the **Beam Pro** console.  
2. Wear the **XReal Air 2 Ultra glasses**.  
3. Select modules from the in-app menu panel:  
   - **Coordinate Tasks**: Place points on the holographic cadaver head.  
   - **ROI Tasks**: Trace and measure areas for accuracy assessment.  
4. Hand-tracking allows real-time annotations directly on holographic anatomy.  
5. Quantitative metrics (distance error, ROI overlap) are displayed for evaluation.  

## Citation
If you use this code or dataset, please cite:  
Tipirneni Y, Agape-Toupadakis Skouritakis C, Blandino A, Arzi B, Goldschmidt S. *Pilot study to assess the utility of augmented reality (AR) glasses for spatial tracking and intraoperative annotation in veterinary oral oncologic surgery.* Front Vet Sci. 2025; in press.

(DOI to be assigned)  

## License
This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.
