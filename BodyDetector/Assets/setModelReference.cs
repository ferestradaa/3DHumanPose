using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class setModelReference : MonoBehaviour {

    // Transforms asignados en el inspector
    public Transform shoulderL, forearmL, handL, shoulderR, forearmR, handR;
    public Transform upperLegL, lowerLegL, footL, upperLegR, lowerLegR, footR;
    public Transform hip;

    //Lists to get base directions
    private List<Vector3> realDirections = new List<Vector3>();
    private List<Vector3> avatarDirections = new List<Vector3>();
    private List<Quaternion> offset = new List<Quaternion>();

    //List to get actual frame pose 
    private List<Vector3> actualPose = new List<Vector3>(); 

    // Listas de Transforms en orden (importante para cálculos)
    private List<Transform> bodyPartsList = new List<Transform>();
    private List<Transform> bodyPartsAux = new List<Transform>();

    // Ejemplo de mapeo para segmentos del avatar
    private Dictionary<string, int> avatarMapping = new Dictionary<string, int> {
        { "shoulderL", 0 },    // shoulderL -> forearmL
        { "forearmL", 1 },     // forearmL -> handL
        { "shoulderR", 3 },    // shoulderR -> forearmR
        { "forearmR", 4 },     // forearmR -> handR
        { "upperLegL", 6 },    // upperLegL -> lowerLegL
        { "lowerLegL", 7 },    // lowerLegL -> footL
        { "upperLegR", 9 },    // upperLegR -> lowerLegR
        { "lowerLegR", 10 }    // lowerLegR -> footR
    };

    void Awake() {
        // Asigna los transforms en el orden correcto
        bodyPartsList = new List<Transform> {
            shoulderL, forearmL, handL,
            shoulderR, forearmR, handR,
            upperLegL, lowerLegL, footL,
            upperLegR, lowerLegR, footR,
            hip
        };

        bodyPartsAux = new List<Transform> {
            shoulderL, forearmL,
            shoulderR, forearmR,
            upperLegL, lowerLegL,
            upperLegR, lowerLegR,
        };
    }

    void Start() {
        actualPose = SetBodyPositions();
        realDirections = realTposeDirections(actualPose);
        avatarDirections = avatarTposeDirections();
        offset = computeOffset(avatarDirections, realDirections); 
  
    }

    public void Execute(){
        Awake(); 
        Start(); 
    }

    public List<Vector3> getRealTposeDirections() => realDirections;
    public List<Vector3> getAvatarTposeDirections() => avatarDirections;
    public List<Quaternion> getOffsets() => offset;

    // Asigna posiciones de los landmarks en orden (asegúrate del orden correcto)
    List<Vector3> SetBodyPositions() {
            // Si el orden es importante, considera usar una lista en lugar de un Dictionary
            List<Vector3> bodyPositionsOrdered = new List<Vector3> {
                new Vector3(0.1773f,  0.2844f,  3.2182f), // leftShoulderPos
                new Vector3(0.3894f,  0.2464f,  3.2300f), // leftElbowPos
                new Vector3(0.6194f,  0.2052f,  3.2384f), // leftWristPos
                new Vector3(-0.1661f, 0.2824f,  3.2009f),  // rightShoulderPos
                new Vector3(-0.3970f, 0.2533f,  3.2214f),  // rightElbowPos
                new Vector3(-0.5850f, 0.2193f,  3.2406f),  // rightWristPos
                new Vector3(0.0964f, -0.2228f,  3.3643f),  // leftUpperLegPos
                new Vector3(0.0568f, -0.6354f,  3.3562f),  // leftLowerLegPos
                new Vector3(0.0690f, -0.9746f,  3.3535f),  // leftFootPos
                new Vector3(-0.1252f,-0.2150f,  3.3633f),  // rightUpperLegPos
                new Vector3(-0.0809f,-0.6035f,  3.3532f),  // rightLowerLegPos
                new Vector3(-0.0462f,-0.9477f,  3.3536f),  // rightFootPos
                new Vector3(-0.0342f,-0.4252f,  3.3747f)   // hip
            };

        
            return bodyPositionsOrdered; 
        }

    List<Vector3> avatarTposeDirections() {
        List<Vector3> directions = new List<Vector3>();
        foreach (var kvp in avatarMapping) {
            int idx = kvp.Value;
            if (idx + 1 < bodyPartsList.Count) {
                Vector3 dir = (bodyPartsList[idx + 1].position - bodyPartsList[idx].position).normalized;
                directions.Add(dir);
            }
        }
        //Debug.Log("Avatar T-Pose: " + string.Join(", ", directions.Select(v => v.ToString())));
        return directions;
    }


    List<Vector3> realTposeDirections(List<Vector3> bodyPositionsOrdered) {
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
                // Si es un segmento de brazo, invertir el vector
                if(segment.Key == "leftArm" || segment.Key == "rightArm") {
                    diff = -diff;
                }
                directions.Add(diff);
            }
        }

        //Debug.Log("Real T-Pose Directions: " + string.Join(", ", directions.Select(v => v.ToString())));
        return directions;
    }


    List<Quaternion> computeOffset(List<Vector3> avatarDirections, List<Vector3> realDirections){
        List<Quaternion> rotations = new List<Quaternion>();
        for (int i= 0; i <Math.Min(bodyPartsAux.Count, Math.Min(avatarDirections.Count, realDirections.Count)); i++){
            Quaternion rotation = Quaternion.FromToRotation(avatarDirections[i], realDirections[i]); 
            bodyPartsAux[i].localRotation =  rotation * bodyPartsAux[i].localRotation;  

            rotations.Add(rotation); 
        }
        //Debug.Log("Offsets: " + string.Join(", ", offset.Select(v => v.ToString())));
        return rotations; 
    }
}
