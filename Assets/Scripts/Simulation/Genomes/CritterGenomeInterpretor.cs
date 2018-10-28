using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterGenomeInterpretor {

    public struct BrushPoint {
        public Vector3 initCoordsNormalized;
        public Vector2 uv;
        public Vector3 curCoords;
        public Vector3 normal;
        public Vector3 tangent;
    }

    

    // MASK TYPES:
    public struct MaskDataLinearFalloff {
        public float slope;
    }
    public struct MaskDataInvDistFalloff {

    }
    public struct MaskDataUniformDir {

    }    
    public struct MaskDataSin {
        public float modifierCenter;
        public float modifierFalloff;
        public float modifierFrequency;
        public float modifierAmplitude;
        public float modifierPhase;
    }
    public struct FalloffMaskDataGeneric {
        public float modifierCenter;
        public float modifierFalloffRate;
        public float modifierFrequency;
        public float modifierAmplitude;
        public float modifierPhase;
    }
    public struct MaskDataLengthwiseEdges {

    }


    public CritterGenomeInterpretor() {
        // empty constructor
    }

    // static helper methods :::: ?

    public static BrushPoint ProcessBrushPoint(BrushPoint inPoint, AgentGenome genome) {

        float fullsizeCritterLength = GetCritterFullsizeLength(genome);
        Vector2 CSScale = GetInitCrossSectionScale(inPoint.initCoordsNormalized.z, genome);
        Vector3 bindPos = new Vector3(inPoint.initCoordsNormalized.x * CSScale.x, inPoint.initCoordsNormalized.y * CSScale.y, inPoint.initCoordsNormalized.z * fullsizeCritterLength - fullsizeCritterLength * 0.5f); //coords;
        inPoint.curCoords = bindPos;
        return inPoint;
    }

    public static Vector3 GetBindPosFromNormalizedCoords(Vector3 coords, AgentGenome genome) {
        //float tempDist = 1f - Mathf.Clamp01(Mathf.Abs(0.5f - coords.z) * 2f);
        //return tempDist;
        float fullsizeCritterLength = GetCritterFullsizeLength(genome);
        //float radius = 1f - Mathf.Clamp01(Mathf.Abs(0.5f - coords.z) * 2f);

        Vector2 CSScale = GetInitCrossSectionScale(coords.z, genome);

        // *** Instead of passing Vector3, create a class that holds extra data:
        // UV, Normal, Tangent, BiTangent, etc.
        // *** and pass that through the various modifier functions

        Vector3 bindPos = new Vector3(coords.x * CSScale.x, coords.y * CSScale.y, coords.z * fullsizeCritterLength - fullsizeCritterLength * 0.5f); //coords;

        return bindPos;
    }
    // GetWidthAtSpineLoc:  
    private static Vector2 GetInitCrossSectionScale(float zCoordNormalized, AgentGenome genome) {

        float fullsizeCritterLength = GetCritterFullsizeLength(genome);
        float bindPoseZ = zCoordNormalized * fullsizeCritterLength;

        float width = 1f;
        float height = 1f;

        if(bindPoseZ > genome.bodyGenome.coreGenome.tailLength) {
            if(bindPoseZ > genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.tailLength) {
                if(bindPoseZ > genome.bodyGenome.coreGenome.headLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.tailLength) {
                    // this point is in the MOUTH Section:
                    float subSectionCoords01 = (bindPoseZ - genome.bodyGenome.coreGenome.tailLength - genome.bodyGenome.coreGenome.bodyLength - genome.bodyGenome.coreGenome.headLength) / genome.bodyGenome.coreGenome.mouthLength; // divide by zero if no tail?                    
                    float sectionBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.mouthToHeadTransitionSize * 0.5f - (subSectionCoords01)) / (genome.bodyGenome.coreGenome.mouthToHeadTransitionSize * 0.5f)) * 0.5f;

                    float headSectionWidth = genome.bodyGenome.coreGenome.bodyBackWidth;
                    float mouthSectionWidth = Mathf.Lerp(genome.bodyGenome.coreGenome.mouthBackWidth, genome.bodyGenome.coreGenome.mouthFrontWidth, subSectionCoords01);
                    width = Mathf.Lerp(mouthSectionWidth, headSectionWidth, sectionBlendLerpAmount);

                    float headSectionHeight = genome.bodyGenome.coreGenome.bodyBackHeight;
                    float mouthSectionHeight = Mathf.Lerp(genome.bodyGenome.coreGenome.mouthBackHeight, genome.bodyGenome.coreGenome.mouthFrontHeight, subSectionCoords01);
                    height = Mathf.Lerp(mouthSectionHeight, headSectionHeight, sectionBlendLerpAmount);

                    if (bindPoseZ > fullsizeCritterLength) { // ERROR                        
                        Debug.LogError("bindPoseZ is longer than the creature is! ");
                    }
                }
                else {
                    // this point is in the HEAD Section:
                    float subSectionCoords01 = (bindPoseZ - genome.bodyGenome.coreGenome.tailLength - genome.bodyGenome.coreGenome.bodyLength) / genome.bodyGenome.coreGenome.headLength;    
                    float bodyBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.headToBodyTransitionSize * 0.5f - subSectionCoords01) / (genome.bodyGenome.coreGenome.headToBodyTransitionSize * 0.5f)) * 0.5f;
                    float mouthBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.mouthToHeadTransitionSize * 0.5f - (1f - subSectionCoords01)) / (genome.bodyGenome.coreGenome.mouthToHeadTransitionSize * 0.5f)) * 0.5f;

                    float bodySectionWidth = genome.bodyGenome.coreGenome.bodyFrontWidth;
                    float headSectionWidth = Mathf.Lerp(genome.bodyGenome.coreGenome.headBackWidth, genome.bodyGenome.coreGenome.headFrontWidth, subSectionCoords01);
                    float mouthSectionWidth = genome.bodyGenome.coreGenome.mouthBackWidth;
                    width = Mathf.Lerp(headSectionWidth, bodySectionWidth, bodyBlendLerpAmount);
                    width = Mathf.Lerp(width, mouthSectionWidth, mouthBlendLerpAmount);

                    float bodySectionHeight = genome.bodyGenome.coreGenome.bodyFrontHeight;
                    float headSectionHeight = Mathf.Lerp(genome.bodyGenome.coreGenome.headBackHeight, genome.bodyGenome.coreGenome.headFrontHeight, subSectionCoords01);
                    float mouthSectionHeight = genome.bodyGenome.coreGenome.mouthBackHeight;
                    height = Mathf.Lerp(headSectionHeight, bodySectionHeight, bodyBlendLerpAmount);
                    height = Mathf.Lerp(height, mouthSectionHeight, mouthBlendLerpAmount);
                }
            }
            else {
                // this point is in the BODY Section:
                float subSectionCoords01 = (bindPoseZ - genome.bodyGenome.coreGenome.tailLength) / genome.bodyGenome.coreGenome.bodyLength;                
                float tailBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.bodyToTailTransitionSize * 0.5f - subSectionCoords01) / (genome.bodyGenome.coreGenome.bodyToTailTransitionSize * 0.5f)) * 0.5f;
                float headBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.headToBodyTransitionSize * 0.5f - (1f - subSectionCoords01)) / (genome.bodyGenome.coreGenome.headToBodyTransitionSize * 0.5f)) * 0.5f;

                float tailSectionWidth = genome.bodyGenome.coreGenome.tailFrontWidth;
                float bodySectionWidth = Mathf.Lerp(genome.bodyGenome.coreGenome.bodyBackWidth, genome.bodyGenome.coreGenome.bodyFrontWidth, subSectionCoords01);
                float headSectionWidth = genome.bodyGenome.coreGenome.headBackWidth;                
                width = Mathf.Lerp(bodySectionWidth, tailSectionWidth, tailBlendLerpAmount);
                width = Mathf.Lerp(width, headSectionWidth, headBlendLerpAmount);

                float tailSectionHeight = genome.bodyGenome.coreGenome.tailFrontHeight;
                float bodySectionHeight = Mathf.Lerp(genome.bodyGenome.coreGenome.bodyBackHeight, genome.bodyGenome.coreGenome.bodyFrontHeight, subSectionCoords01);
                float headSectionHeight = genome.bodyGenome.coreGenome.headBackHeight;                
                height = Mathf.Lerp(bodySectionHeight, tailSectionHeight, tailBlendLerpAmount);
                height = Mathf.Lerp(height, headSectionHeight, headBlendLerpAmount);
            }
        }
        else {
            // this point is in the TAIL Section:
            float subSectionCoords01 = bindPoseZ / genome.bodyGenome.coreGenome.tailLength; // divide by zero if no tail?            
            float sectionBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.bodyToTailTransitionSize * 0.5f - (1f - subSectionCoords01)) / (genome.bodyGenome.coreGenome.bodyToTailTransitionSize * 0.5f)) * 0.5f;

            float tailSectionWidth = Mathf.Lerp(genome.bodyGenome.coreGenome.tailBackWidth, genome.bodyGenome.coreGenome.tailFrontWidth, subSectionCoords01);
            float bodySectionWidth = genome.bodyGenome.coreGenome.bodyBackWidth;
            width = Mathf.Lerp(tailSectionWidth, bodySectionWidth, sectionBlendLerpAmount);

            float tailSectionHeight = Mathf.Lerp(genome.bodyGenome.coreGenome.tailBackHeight, genome.bodyGenome.coreGenome.tailFrontHeight, subSectionCoords01);
            float bodySectionHeight = genome.bodyGenome.coreGenome.bodyBackHeight;
            height = Mathf.Lerp(tailSectionHeight, bodySectionHeight, sectionBlendLerpAmount);
        }


        Vector2 crossSectionScale = new Vector2(width, height);

        // Now Body Modifiers are processed:
        for(int i = 0; i < genome.bodyGenome.coreGenome.shapeModifiersList.Count; i++) {
            if(genome.bodyGenome.coreGenome.shapeModifiersList[i].modifierTypeID == 0) {  // extrude
                if(genome.bodyGenome.coreGenome.shapeModifiersList[i].modifierCoordinateTypeID == 0) {  // lengthwise

                    // Get coordData:
                    CritterModuleCoreGenome.ModifierCoordinateData coordData = genome.bodyGenome.coreGenome.modifierCoordinateDataList[genome.bodyGenome.coreGenome.shapeModifiersList[i].modifierCoordinateDataIndex];

                    if(genome.bodyGenome.coreGenome.shapeModifiersList[i].maskFalloffTypeID == 0) {  // linear falloff curve
                        FalloffMaskDataGeneric falloffCurveData = genome.bodyGenome.coreGenome.maskDataListGeneric[genome.bodyGenome.coreGenome.shapeModifiersList[i].maskDataIndex];

                        float distToModCenter = zCoordNormalized - falloffCurveData.modifierCenter;
                        float modStrength = Mathf.Clamp01(1f - distToModCenter * falloffCurveData.modifierFalloffRate);
                        float radiusMult = modStrength * falloffCurveData.modifierAmplitude + 1f;
                        crossSectionScale *= radiusMult;
                    }

                    if(genome.bodyGenome.coreGenome.shapeModifiersList[i].maskFalloffTypeID == 2) {  // Sin() falloff curve
                        FalloffMaskDataGeneric falloffCurveData = genome.bodyGenome.coreGenome.maskDataListGeneric[genome.bodyGenome.coreGenome.shapeModifiersList[i].maskDataIndex];
                        float distToModCenter = zCoordNormalized - falloffCurveData.modifierCenter;
                        float modStrength = Mathf.Clamp01(1f - falloffCurveData.modifierFalloffRate);
                        float radiusMult = Mathf.Sin((distToModCenter + falloffCurveData.modifierPhase) * falloffCurveData.modifierFrequency) * falloffCurveData.modifierAmplitude + 1f;
                        crossSectionScale *= radiusMult;
                    }
                }
            }
        }

        // SIN:        
        //float distToModCenter = zCoordNormalized - genome.bodyGenome.coreGenome.maskDataSinTemp.modifierCenter;
        //float modStrength = Mathf.Clamp01(1f - genome.bodyGenome.coreGenome.maskDataSinTemp.modifierFalloff);
        //float radiusMult = Mathf.Sin((distToModCenter + genome.bodyGenome.coreGenome.maskDataSinTemp.modifierPhase) * genome.bodyGenome.coreGenome.maskDataSinTemp.modifierFrequency) * genome.bodyGenome.coreGenome.maskDataSinTemp.modifierAmplitude + 1f;
        //crossSectionScale *= radiusMult;
        
        return crossSectionScale;
    }
    private float GetCrossSectionRadiusAtCoords(float zCoordNormalized) {
        return 1f;
    }
    private static float GetCritterFullsizeLength(AgentGenome genome) {
        return genome.bodyGenome.coreGenome.mouthLength + genome.bodyGenome.coreGenome.headLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.tailLength;
    }

    // Read genome, spit out renderBuffer data for different render passes:

    // Read genome, spit out collider object positions, sizes, orientations:

    // Create mutated copy of input Genome:

}
