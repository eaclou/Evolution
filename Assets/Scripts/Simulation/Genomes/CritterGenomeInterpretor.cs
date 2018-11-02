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

    /// <summary>
    /// COORDINATES FOR CREATURES:
    /// Positive Y = forward, creature head
    /// X Axis = Creature width;
    /// Negative Z = world UP!  note that this is flipped
    /// </summary>

    private static void ProcessBrushPointBaseBody(ref BrushPoint point, AgentGenome genome) {
        CritterModuleCoreGenome gene = genome.bodyGenome.coreGenome; // for readability

        float segmentsSummedCritterLength = GetCritterFullsizeLength(genome);
        float bindPoseY = point.initCoordsNormalized.y * segmentsSummedCritterLength;

        float widthMultiplier = 1f;
        float heightMultiplier = 1f;

        if(bindPoseY > gene.tailLength) {
            if(bindPoseY > gene.bodyLength + gene.tailLength) {
                if(bindPoseY > gene.headLength + gene.bodyLength + gene.tailLength) {
                    // this point is in the MOUTH Section:
                    float subSectionCoords01 = (bindPoseY - gene.tailLength - gene.bodyLength - gene.headLength) / gene.mouthLength;
                    float mouthTipBlendLerpAmount = Mathf.Clamp01((gene.mouthEndCapTaperSize * 0.5f - (1f - subSectionCoords01)) / (gene.mouthEndCapTaperSize * 0.5f));
                    float headBlendLerpAmount = Mathf.Clamp01((gene.mouthToHeadTransitionSize * 0.5f - (subSectionCoords01)) / (gene.mouthToHeadTransitionSize * 0.5f)) * 0.5f;

                    float headSectionWidth = gene.bodyBackWidth;
                    float mouthSectionWidth = Mathf.Lerp(gene.mouthBackWidth, gene.mouthFrontWidth, subSectionCoords01);
                    widthMultiplier = Mathf.Lerp(mouthSectionWidth, headSectionWidth, headBlendLerpAmount);
                    widthMultiplier = Mathf.Lerp(widthMultiplier, 0f, Mathf.Pow(mouthTipBlendLerpAmount, 1f));
                    //widthMultiplier = widthMultiplier * gene.mouthLength;

                    float headSectionHeight = gene.bodyBackHeight;
                    float mouthSectionHeight = Mathf.Lerp(gene.mouthBackHeight, gene.mouthFrontHeight, subSectionCoords01);
                    heightMultiplier = Mathf.Lerp(mouthSectionHeight, headSectionHeight, headBlendLerpAmount);
                    heightMultiplier = Mathf.Lerp(heightMultiplier, 0f, Mathf.Pow(mouthTipBlendLerpAmount, 1f));
                    //heightMultiplier = heightMultiplier * gene.mouthLength;

                    if (bindPoseY > segmentsSummedCritterLength) { // ERROR                        
                        Debug.LogError("bindPoseY is longer than the creature is! ");
                    }
                }
                else {
                    // this point is in the HEAD Section:
                    float subSectionCoords01 = (bindPoseY - gene.tailLength - gene.bodyLength) / gene.headLength;    
                    float bodyBlendLerpAmount = Mathf.Clamp01((gene.headToBodyTransitionSize * 0.5f - subSectionCoords01) / (gene.headToBodyTransitionSize * 0.5f)) * 0.5f;
                    float mouthBlendLerpAmount = Mathf.Clamp01((gene.mouthToHeadTransitionSize * 0.5f - (1f - subSectionCoords01)) / (gene.mouthToHeadTransitionSize * 0.5f)) * 0.5f;

                    float bodySectionWidth = gene.bodyFrontWidth;
                    float headSectionWidth = Mathf.Lerp(gene.headBackWidth, gene.headFrontWidth, subSectionCoords01);
                    float mouthSectionWidth = gene.mouthBackWidth;
                    widthMultiplier = Mathf.Lerp(headSectionWidth, bodySectionWidth, bodyBlendLerpAmount);
                    widthMultiplier = Mathf.Lerp(widthMultiplier, mouthSectionWidth, mouthBlendLerpAmount);
                    //widthMultiplier = widthMultiplier * gene.headLength;

                    float bodySectionHeight = gene.bodyFrontHeight;
                    float headSectionHeight = Mathf.Lerp(gene.headBackHeight, gene.headFrontHeight, subSectionCoords01);
                    float mouthSectionHeight = gene.mouthBackHeight;
                    heightMultiplier = Mathf.Lerp(headSectionHeight, bodySectionHeight, bodyBlendLerpAmount);
                    heightMultiplier = Mathf.Lerp(heightMultiplier, mouthSectionHeight, mouthBlendLerpAmount);
                    //heightMultiplier = heightMultiplier * gene.headLength;
                }
            }
            else {
                // this point is in the BODY Section:
                float subSectionCoords01 = (bindPoseY - gene.tailLength) / gene.bodyLength;                
                float tailBlendLerpAmount = Mathf.Clamp01((gene.bodyToTailTransitionSize * 0.5f - subSectionCoords01) / (gene.bodyToTailTransitionSize * 0.5f)) * 0.5f;
                float headBlendLerpAmount = Mathf.Clamp01((gene.headToBodyTransitionSize * 0.5f - (1f - subSectionCoords01)) / (gene.headToBodyTransitionSize * 0.5f)) * 0.5f;

                float tailSectionWidth = gene.tailFrontWidth;
                float bodySectionWidth = Mathf.Lerp(gene.bodyBackWidth, gene.bodyFrontWidth, subSectionCoords01);
                float headSectionWidth = gene.headBackWidth;                
                widthMultiplier = Mathf.Lerp(bodySectionWidth, tailSectionWidth, tailBlendLerpAmount);
                widthMultiplier = Mathf.Lerp(widthMultiplier, headSectionWidth, headBlendLerpAmount);
                //widthMultiplier = widthMultiplier * gene.bodyLength;

                float tailSectionHeight = gene.tailFrontHeight;
                float bodySectionHeight = Mathf.Lerp(gene.bodyBackHeight, gene.bodyFrontHeight, subSectionCoords01);
                float headSectionHeight = gene.headBackHeight;                
                heightMultiplier = Mathf.Lerp(bodySectionHeight, tailSectionHeight, tailBlendLerpAmount);
                heightMultiplier = Mathf.Lerp(heightMultiplier, headSectionHeight, headBlendLerpAmount);
                //heightMultiplier = heightMultiplier * gene.bodyLength;
            }
        }
        else {
            // this point is in the TAIL Section:
            float subSectionCoords01 = bindPoseY / gene.tailLength; // divide by zero if no tail?
            float bodyBlendLerpAmount = Mathf.Clamp01((gene.bodyToTailTransitionSize * 0.5f - (1f - subSectionCoords01)) / (gene.bodyToTailTransitionSize * 0.5f)) * 0.5f;
            float tailTipBlendLerpAmount = Mathf.Clamp01((gene.tailEndCapTaperSize - subSectionCoords01) / (gene.tailEndCapTaperSize));

            float tailSectionWidth = Mathf.Lerp(gene.tailBackWidth, gene.tailFrontWidth, subSectionCoords01);
            float bodySectionWidth = gene.bodyBackWidth;
            widthMultiplier = Mathf.Lerp(tailSectionWidth, bodySectionWidth, bodyBlendLerpAmount);
            widthMultiplier = Mathf.Lerp(widthMultiplier, 0f, tailTipBlendLerpAmount);
            //widthMultiplier = widthMultiplier * gene.tailLength;

            float tailSectionHeight = Mathf.Lerp(gene.tailBackHeight, gene.tailFrontHeight, subSectionCoords01);
            float bodySectionHeight = gene.bodyBackHeight;
            heightMultiplier = Mathf.Lerp(tailSectionHeight, bodySectionHeight, bodyBlendLerpAmount);
            heightMultiplier = Mathf.Lerp(heightMultiplier, 0f, tailTipBlendLerpAmount);
            //heightMultiplier = heightMultiplier * gene.tailLength;
        }

        //Vector2 crossSectionScale = new Vector2(widthMultiplier, heightMultiplier);
        //float crossSectionWidth = 

        // Now Body Modifiers are processed:
        for(int i = 0; i < gene.shapeModifiersList.Count; i++) {
            CritterModuleCoreGenome.ShapeModifierData modifierData = gene.shapeModifiersList[i];
            if(modifierData.modifierTypeID == CritterModuleCoreGenome.ShapeModifierType.Extrude) {  // extrude
                // Find extrude amount:
                float maskValue = 1f;

                for(int j = 0; j < gene.shapeModifiersList[i].maskIndicesList.Count; j++) {
                    int maskIndex = gene.shapeModifiersList[i].maskIndicesList[j];
                    CritterModuleCoreGenome.MaskData maskData = gene.masksList[maskIndex];

                    float rawMaskValue = GetMaskValue(point, maskData);

                    // taper distance?:
                    float taperMask = 1f - Mathf.Clamp01(Mathf.Abs(point.initCoordsNormalized.y - maskData.origin) / modifierData.taperDistance);

                    maskValue *= rawMaskValue * taperMask;
                    
                }
                
                float radiusMult = maskValue * modifierData.amplitude + 1f;
                widthMultiplier *= radiusMult;
                heightMultiplier *= radiusMult;
            }
        }

        float finalCreatureLength = segmentsSummedCritterLength * gene.creatureBaseLength;
        float finalCreatureThickness = 1f / gene.creatureBaseAspectRatio * finalCreatureLength;
        point.bindPos = new Vector3(point.initCoordsNormalized.x * widthMultiplier * finalCreatureThickness, point.initCoordsNormalized.y * finalCreatureLength - finalCreatureLength * 0.5f, point.initCoordsNormalized.z * heightMultiplier * finalCreatureThickness);
    }

    private static float GetMaskValue(BrushPoint point, CritterModuleCoreGenome.MaskData maskData) {        
        float outValue = 0f;
        float inValue = 0f;

        // MASK COORDINATE SPACES:
        if (maskData.coordinateTypeID == CritterModuleCoreGenome.MaskCoordinateType.Lengthwise) {
            inValue = Mathf.Abs(point.initCoordsNormalized.y - maskData.origin) / maskData.cycleDistance;
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

            float dot = Vector2.Dot(new Vector2(point.initCoordsNormalized.x, point.initCoordsNormalized.z).normalized, maskData.axisDir.normalized);  // *** IF I CHANGE coordinate Axes this needs to change!! ***

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
        CritterModuleCoreGenome gene = genome.bodyGenome.coreGenome;
        return (gene.mouthLength + gene.headLength + gene.bodyLength + gene.tailLength); // * gene.creatureBaseLength;
    }

    // Read genome, spit out renderBuffer data for different render passes:

    // Read genome, spit out collider object positions, sizes, orientations:

    // Create mutated copy of input Genome:

}
