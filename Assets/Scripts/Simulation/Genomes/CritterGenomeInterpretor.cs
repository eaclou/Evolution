using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterGenomeInterpretor {

    public struct BrushPoint {
        public int ix;
        public int iy;
        public Vector3 initCoordsNormalized;
        public Vector2 uv;
        public Vector3 bindPos;
        public Vector3 normal;
        public Vector3 tangent;
    }

    public CritterGenomeInterpretor() {
        // empty constructor
    }

    // static helper methods :::: ?

    public static BrushPoint ProcessBrushPoint(BrushPoint inPoint, AgentGenome genome) {

        ProcessBrushPointBaseBody(ref inPoint, genome); // basic proportions

        // calculate normals & stuff:
        // Smooth & fix overly jagged/weird spots/shapes:

        // Colors?

        // Extras:
        // fins, etc.


        // OLD:
        //Vector2 CSScale = GetInitCrossSectionScale(inPoint.initCoordsNormalized.z, genome);
        //Vector3 bindPos = new Vector3(inPoint.initCoordsNormalized.x * CSScale.x, inPoint.initCoordsNormalized.y * CSScale.y, inPoint.initCoordsNormalized.z * fullsizeCritterLength - fullsizeCritterLength * 0.5f); //coords;
        //inPoint.bindPos = bindPos;
        return inPoint;
    }

    private static void CalculateBrushNormals() {

    }

    private static void ProcessBrushPointBaseBody(ref BrushPoint point, AgentGenome genome) {
        float fullsizeCritterLength = GetCritterFullsizeLength(genome);
        float bindPoseZ = point.initCoordsNormalized.z * fullsizeCritterLength;

        float width = 1f;
        float height = 1f;

        if(bindPoseZ > genome.bodyGenome.coreGenome.tailLength) {
            if(bindPoseZ > genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.tailLength) {
                if(bindPoseZ > genome.bodyGenome.coreGenome.headLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.tailLength) {
                    // this point is in the MOUTH Section:
                    float subSectionCoords01 = (bindPoseZ - genome.bodyGenome.coreGenome.tailLength - genome.bodyGenome.coreGenome.bodyLength - genome.bodyGenome.coreGenome.headLength) / genome.bodyGenome.coreGenome.mouthLength;
                    float mouthTipBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.mouthToTipTransitionSize * 0.5f - (1f - subSectionCoords01)) / (genome.bodyGenome.coreGenome.mouthToTipTransitionSize * 0.5f));
                    float headBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.mouthToHeadTransitionSize * 0.5f - (subSectionCoords01)) / (genome.bodyGenome.coreGenome.mouthToHeadTransitionSize * 0.5f)) * 0.5f;

                    float headSectionWidth = genome.bodyGenome.coreGenome.bodyBackWidth;
                    float mouthSectionWidth = Mathf.Lerp(genome.bodyGenome.coreGenome.mouthBackWidth, genome.bodyGenome.coreGenome.mouthFrontWidth, subSectionCoords01);
                    width = Mathf.Lerp(mouthSectionWidth, headSectionWidth, headBlendLerpAmount);
                    width = Mathf.Lerp(width, 0f, Mathf.Pow(mouthTipBlendLerpAmount, 2.5f));
                    width = width * genome.bodyGenome.coreGenome.mouthLength;

                    float headSectionHeight = genome.bodyGenome.coreGenome.bodyBackHeight;
                    float mouthSectionHeight = Mathf.Lerp(genome.bodyGenome.coreGenome.mouthBackHeight, genome.bodyGenome.coreGenome.mouthFrontHeight, subSectionCoords01);
                    height = Mathf.Lerp(mouthSectionHeight, headSectionHeight, headBlendLerpAmount);
                    height = Mathf.Lerp(height, 0f, Mathf.Pow(mouthTipBlendLerpAmount, 2.5f));
                    height = height * genome.bodyGenome.coreGenome.mouthLength;

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
                    width = width * genome.bodyGenome.coreGenome.headLength;

                    float bodySectionHeight = genome.bodyGenome.coreGenome.bodyFrontHeight;
                    float headSectionHeight = Mathf.Lerp(genome.bodyGenome.coreGenome.headBackHeight, genome.bodyGenome.coreGenome.headFrontHeight, subSectionCoords01);
                    float mouthSectionHeight = genome.bodyGenome.coreGenome.mouthBackHeight;
                    height = Mathf.Lerp(headSectionHeight, bodySectionHeight, bodyBlendLerpAmount);
                    height = Mathf.Lerp(height, mouthSectionHeight, mouthBlendLerpAmount);
                    height = height * genome.bodyGenome.coreGenome.headLength;
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
                width = width * genome.bodyGenome.coreGenome.bodyLength;

                float tailSectionHeight = genome.bodyGenome.coreGenome.tailFrontHeight;
                float bodySectionHeight = Mathf.Lerp(genome.bodyGenome.coreGenome.bodyBackHeight, genome.bodyGenome.coreGenome.bodyFrontHeight, subSectionCoords01);
                float headSectionHeight = genome.bodyGenome.coreGenome.headBackHeight;                
                height = Mathf.Lerp(bodySectionHeight, tailSectionHeight, tailBlendLerpAmount);
                height = Mathf.Lerp(height, headSectionHeight, headBlendLerpAmount);
                height = height * genome.bodyGenome.coreGenome.bodyLength;
            }
        }
        else {
            // this point is in the TAIL Section:
            float subSectionCoords01 = bindPoseZ / genome.bodyGenome.coreGenome.tailLength; // divide by zero if no tail?
            float bodyBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.bodyToTailTransitionSize * 0.5f - (1f - subSectionCoords01)) / (genome.bodyGenome.coreGenome.bodyToTailTransitionSize * 0.5f)) * 0.5f;
            float tailTipBlendLerpAmount = Mathf.Clamp01((genome.bodyGenome.coreGenome.tailToTipTransitionSize * 0.5f - subSectionCoords01) / (genome.bodyGenome.coreGenome.tailToTipTransitionSize * 0.5f));

            float tailSectionWidth = Mathf.Lerp(genome.bodyGenome.coreGenome.tailBackWidth, genome.bodyGenome.coreGenome.tailFrontWidth, subSectionCoords01);
            float bodySectionWidth = genome.bodyGenome.coreGenome.bodyBackWidth;
            width = Mathf.Lerp(tailSectionWidth, bodySectionWidth, bodyBlendLerpAmount);
            width = Mathf.Lerp(width, 0f, tailTipBlendLerpAmount);
            width = width * genome.bodyGenome.coreGenome.tailLength;

            float tailSectionHeight = Mathf.Lerp(genome.bodyGenome.coreGenome.tailBackHeight, genome.bodyGenome.coreGenome.tailFrontHeight, subSectionCoords01);
            float bodySectionHeight = genome.bodyGenome.coreGenome.bodyBackHeight;
            height = Mathf.Lerp(tailSectionHeight, bodySectionHeight, bodyBlendLerpAmount);
            height = Mathf.Lerp(height, 0f, tailTipBlendLerpAmount);
            height = height * genome.bodyGenome.coreGenome.tailLength;
        }

        Vector2 crossSectionScale = new Vector2(width, height);


        // Now Body Modifiers are processed:
        for(int i = 0; i < genome.bodyGenome.coreGenome.shapeModifiersList.Count; i++) {
            CritterModuleCoreGenome.ShapeModifierData modifierData = genome.bodyGenome.coreGenome.shapeModifiersList[i];
            if(modifierData.modifierTypeID == CritterModuleCoreGenome.ShapeModifierType.Extrude) {  // extrude
                // Find extrude amount:
                float maskValue = 1f;

                for(int j = 0; j < genome.bodyGenome.coreGenome.shapeModifiersList[i].maskIndicesList.Count; j++) {
                    int maskIndex = genome.bodyGenome.coreGenome.shapeModifiersList[i].maskIndicesList[j];
                    CritterModuleCoreGenome.MaskData maskData = genome.bodyGenome.coreGenome.masksList[maskIndex];

                    float rawMaskValue = GetMaskValue(point, maskData);

                    // taper distance?:
                    float taperMask = 1f - Mathf.Clamp01(Mathf.Abs(point.initCoordsNormalized.z - maskData.origin) / modifierData.taperDistance);

                    maskValue *= rawMaskValue * taperMask;
                    
                }
                
                float radiusMult = maskValue * modifierData.amplitude + 1f;
                crossSectionScale *= radiusMult;
            }
        }

        point.bindPos = new Vector3(point.initCoordsNormalized.x * crossSectionScale.x, point.initCoordsNormalized.y * crossSectionScale.y, point.initCoordsNormalized.z * fullsizeCritterLength - fullsizeCritterLength * 0.5f);
    }

    private static float GetMaskValue(BrushPoint point, CritterModuleCoreGenome.MaskData maskData) {        
        float outValue = 0f;
        float inValue = 0f;

        // MASK COORDINATE SPACES:
        if (maskData.coordinateTypeID == CritterModuleCoreGenome.MaskCoordinateType.Lengthwise) {
            inValue = Mathf.Abs(point.initCoordsNormalized.z - maskData.origin) / maskData.cycleDistance;
        }
        if(maskData.coordinateTypeID == CritterModuleCoreGenome.MaskCoordinateType.Polygonize) {
            //find 2 closest edges
            float pointFaceCoord = point.uv.x * (float)maskData.numPolyEdges;
            
            float dist0 = pointFaceCoord % 1f;
            float dist1 = 1f - dist0;
            
            float dist = Mathf.Max(dist0, dist1) / maskData.cycleDistance;

            inValue = dist;
        }
        if(maskData.coordinateTypeID == CritterModuleCoreGenome.MaskCoordinateType.SingleAxis) {

            float dot = Vector2.Dot(new Vector2(point.initCoordsNormalized.x, point.initCoordsNormalized.y).normalized, maskData.axisDir.normalized);  // *** IF I CHANGE coordinate Axes this needs to change!! ***

            inValue = (dot * 0.5f + 0.5f) / maskData.cycleDistance;
        }

        // FUNCTION TYPES:
        if (maskData.functionTypeID == CritterModuleCoreGenome.MaskFunctionType.Linear) {            
            outValue = Mathf.Clamp01(1f - inValue);
        }
        if (maskData.functionTypeID == CritterModuleCoreGenome.MaskFunctionType.Sin) {            
            outValue = Mathf.Sin(inValue) * Mathf.PI;
        }
        if (maskData.functionTypeID == CritterModuleCoreGenome.MaskFunctionType.Cos) {           
            outValue = Mathf.Cos(inValue) * Mathf.PI;
        }

        return outValue;
    }

    private static float GetCritterFullsizeLength(AgentGenome genome) {
        return genome.bodyGenome.coreGenome.mouthLength + genome.bodyGenome.coreGenome.headLength + genome.bodyGenome.coreGenome.bodyLength + genome.bodyGenome.coreGenome.tailLength;
    }

    // Read genome, spit out renderBuffer data for different render passes:

    // Read genome, spit out collider object positions, sizes, orientations:

    // Create mutated copy of input Genome:

}
