using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class motionCapture : MonoBehaviour{

// Transforms asignados en el inspector
    public Transform shoulderL, forearmL, handL, shoulderR, forearmR, handR;
    public Transform upperLegL, lowerLegL, footL, upperLegR, lowerLegR, footR;
    public Transform hip;

    private setModelReference referenceModel;
    private List<Vector3> realTposeDirections = new List<Vector3>();
    private List<Quaternion> currentOffsets = new List<Quaternion>(); 
    private List<Quaternion> referenceOffsets = new List<Quaternion>(); 

    private List<Vector3> currentLandmarks = new List<Vector3>(); 
    private List<Vector3> currentDirection = new List<Vector3>(); 
    private List<Transform> bodyPartsAux = new List<Transform>();


    void Awake() {
        bodyPartsAux = new List<Transform> {
            shoulderL, forearmL,
            shoulderR, forearmR,
            upperLegL, lowerLegL,
            upperLegR, lowerLegR,
        };
    }

    

    void Start(){
        referenceModel = GetComponent<setModelReference>(); // Buscar en el mismo GameObject

        if (referenceModel != null){
            referenceModel.Execute(); 
        }

        realTposeDirections = referenceModel.getRealTposeDirections(); 
        referenceOffsets = referenceModel.getOffsets(); 
    //}

    //void Update(){
        currentLandmarks = getActualpose(); 
        currentDirection = computeCurrentDirection(currentLandmarks); 
        currentOffsets = computeCurentOffset(realTposeDirections, currentDirection, referenceOffsets); 



    }


    List<Vector3> getActualpose() { //funcion para almacenar los landmarks detectados, solo devuelve una lista con todos
            List<Vector3> bodyPositionsOrdered = new List<Vector3> {
                new Vector3(0.2229f,  0.1796f,  3.1081f), // leftShoulderPos
                new Vector3(0.4069f,  0.1380f,  2.9964f), // leftElbowPos
                new Vector3(0.4586f,  0.2971f,  2.8155f), // leftWristPos
                new Vector3(-0.1104f,  0.1614f,  3.0724f),  // rightShoulderPos
                new Vector3(-0.3244f,  0.1318f,  3.0014f),  // rightElbowPos
                new Vector3(-0.3661f,  0.3034f,  2.8780f),  // rightWristPos
                new Vector3(0.1784f, -0.3078f,  3.1129f),  // leftUpperLegPos
                new Vector3(0.4314f, -0.5278f,  2.9513f),  // leftLowerLegPos
                new Vector3(0.3806f, -0.8354f,  3.1159f),  // leftFootPos
                new Vector3(-0.0506f, -0.3086f,  3.0997f),  // rightUpperLegPos
                new Vector3(-0.2431f, -0.5068f,  2.8976f),  // rightLowerLegPos
                new Vector3(-0.1848f, -0.8446f,  3.0659f),  // rightFootPos
                new Vector3(-0.0342f,-0.4252f,  3.3747f)   // hip
            };

            return bodyPositionsOrdered; 
        }

    List<Vector3> computeCurrentDirection(List<Vector3> bodyPositionsOrdered){
        List<Vector3> directions = new List<Vector3>();
        Dictionary<string, List<int>> segmentMapping = new Dictionary<string, List<int>> {
            { "leftArm", new List<int> { 0, 1, 2 } },       // leftShoulder, leftElbow, leftWrist
            { "rightArm", new List<int> { 3, 4, 5 } },      // rightShoulder, rightElbow, rightWrist
            { "leftLeg", new List<int> { 6, 7, 8 } },       // leftUpperLeg, leftLowerLeg, leftFoot
            { "rightLeg", new List<int> { 9, 10, 11 } }      // rightUpperLeg, rightLowerLeg, rightFoot
        };

        foreach (var segment in segmentMapping) {
            List<int> indices = segment.Value;
            for (int i = 1; i < indices.Count; i++) {
                int prev = indices[i - 1];
                int curr = indices[i];
                Vector3 diff = (bodyPositionsOrdered[curr] - bodyPositionsOrdered[prev]).normalized;
    
                if(segment.Key == "leftArm" || segment.Key == "rightArm") { //invertir el vector en caso de ser del lado derecho
                    diff = -diff;
                }
                directions.Add(diff);
            }
        }

        //Debug.Log("Real T-Pose Directions: " + string.Join(", ", directions.Select(v => v.ToString())));
        return directions;
        }

    List<Quaternion> computeCurentOffset(List<Vector3> realTposeDirections, List<Vector3> currentDirection, List<Quaternion> offsets){
        List<Quaternion> rotations = new List<Quaternion>();
        for (int i= 0; i <Math.Min(bodyPartsAux.Count, Math.Min(realTposeDirections.Count, currentDirection.Count)); i++){
            Quaternion rotation = Quaternion.FromToRotation(realTposeDirections[i], currentDirection[i]); 
            bodyPartsAux[i].localRotation = rotation * bodyPartsAux[i].localRotation;

            rotations.Add(rotation); 
        }
        //Debug.Log("Offsets: " + string.Join(", ", offset.Select(v => v.ToString())));
        return rotations; 
    }

    }


