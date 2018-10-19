using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterGenomeInterpretor {


    public CritterGenomeInterpretor() {
        // empty constructor
    }

    // static helper methods :::: ?

    public static Vector3 GetBindPosFromNormalizedCoords(Vector3 coords, AgentGenome genome) {
        //float tempDist = 1f - Mathf.Clamp01(Mathf.Abs(0.5f - coords.z) * 2f);
        //return tempDist;
        float fullsizeCritterLength = GetCritterFullsizeLength(genome);
        //float radius = 1f - Mathf.Clamp01(Mathf.Abs(0.5f - coords.z) * 2f);

        Vector2 CSScale = GetCrossSectionScale(coords.z, genome);

        Vector3 bindPos = new Vector3(coords.x * CSScale.x, coords.y * CSScale.y, coords.z * fullsizeCritterLength - fullsizeCritterLength * 0.5f); //coords;

        return bindPos;
    }
    // GetWidthAtSpineLoc:  
    private static Vector2 GetCrossSectionScale(float zCoordNormalized, AgentGenome genome) {

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
