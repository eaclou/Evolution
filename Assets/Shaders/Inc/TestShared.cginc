const int hashMask = 255;
const int[512] hash = {
	151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
	140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
	247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
	57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
	74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
	60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
	65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
	200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
	52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
	207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
	119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
	129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
	218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
	81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
	184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
	222,114, 67, 29, 24, 72,141,243,128,195, 78, 66,215, 61,156,180,
		
	151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
	140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
	247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
	57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
	74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
	60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
	65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
	200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
	52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
	207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
	119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
	129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
	218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
	81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
	184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
	222,114, 67, 29, 24, 72,141,243,128,195, 78, 66,215, 61,156,180
};

// Helper Functions:
float Smooth(float t) {
	return t * t * t * (t * (t * 6 - 15) + 10);
}
float SmoothDerivative (float t) {
	return 30 * t * t * (t * (t - 2) + 1f);
}
float Dot(float2 g, float x, float y) {
	return g.x * x + g.y * y;
}
float Dot(float3 g, float x, float y, float z) {
	return g.x * x + g.y * y + g.z * z;
}

float2 Value1D(float point, float frequency) {
	point *= frequency;  
	int i0 = floor(point);  // discard the digits after the decimal place
	float t = point - i0;  // get only the digits after the decimal, the position between the i0 value and the next whole integer, used for interpolating
	float dt = SmoothDerivative (t);
	t = Smooth (t);  // remap the 0-1 t value to a function with slope=0 at end points, to keep function continuous
	i0 &= hashMask;  // make sure i0 integer value falls within the bounds of the hash table, i.e. always between 0-255 in this case
	int i1 = i0 + 1;  // get value of next whole integer

	int h0 = hash [i0];  // hash the floor integer i0 into its noise value
	int h1 = hash [i1]; // hash the next integer i1 into its noise value

	float a = h0;
	float b = h1 - h0;

	float2 sample;
	sample.x = a + b * t;  // blend between the noise values at i0 and i1, based on the smoothed 0-1 curve t
	sample.y = b * dt;
	sample.y *= frequency;
	return sample * (2.0 / hashMask) - 1.0;
}

float3 Value2D(float2 point, float frequency) {
	point *= frequency;
	int ix0 = floor(point.x);  // get floor integer value in X
	int iy0 = floor(point.y); // get floor integer value in Y
	float tx = point.x - ix0;  // get interpolation position in X;  just the fractional part of point.x 
	float ty = point.y - iy0; // get interpolation position in Y;   ''          ''         ''   point.y
	ix0 &= hashMask; // get X floor int position in range of hash table, 0-255
	iy0 &= hashMask; // get Y floor int position in range of hash table, 0-255
	int ix1 = ix0 + 1;  // get X position of next whole integer, floor int + one;
	int iy1 = iy0 + 1;  // get Y position of next whole integer, floor int + one;

	int h0 = hash[ix0];  // get value of hashed Noise at position of floor int ix0
	int h1 = hash[ix1];  // get value of hashed Noise at position of next whole integer, ix1
		// 2D values computed by using 1D hash in X axis as a position offset for the hash of the Y position
	int h00 = hash[h0 + iy0];  // get hash noise value of bottom left corner, (x0, y0)   
	int h10 = hash[h1 + iy0];  // get hash noise value of bottom right corner, (x1, y0)
	int h01 = hash[h0 + iy1];  // get hash noise value of top left corner, (x0, y1)
	int h11 = hash[h1 + iy1];  // get hash noise value of top right corner, (x1, y1)

	float dtx = SmoothDerivative(tx);
	float dty = SmoothDerivative(ty);
	tx = Smooth(tx);  // remap the fractional interpolation value to a continuous curve
	ty = Smooth(ty);

	// // Interpolate between noise values along X axis at y=0 and y=1
	// // Then interpolate between those two values based on the Y axis

	float a = h00;
	float b = h10 - h00;
	float c = h01 - h00;
	float d = h11 - h01 - h10 + h00;

	float3 sample;
	sample.x = a + b * tx + (c + d * tx) * ty;
	sample.y = (b + d * ty) * dtx;
	sample.z = (c + d * tx) * dty;
	sample.y *= frequency;  // derivatives
	sample.z *= frequency;  // derivatives
	return sample * (2.0 / hashMask) - 1.0;
}

float4 Value3D (float3 point, float frequency) {
	point *= frequency;
	int ix0 = floor(point.x);
	int iy0 = floor(point.y);
	int iz0 = floor(point.z);
	float tx = point.x - ix0;
	float ty = point.y - iy0;
	float tz = point.z - iz0;
	ix0 &= hashMask;
	iy0 &= hashMask;
	iz0 &= hashMask;
	int ix1 = ix0 + 1;
	int iy1 = iy0 + 1;
	int iz1 = iz0 + 1;
		
	int h0 = hash[ix0];
	int h1 = hash[ix1];
	int h00 = hash[h0 + iy0];
	int h10 = hash[h1 + iy0];
	int h01 = hash[h0 + iy1];
	int h11 = hash[h1 + iy1];
	int h000 = hash[h00 + iz0];
	int h100 = hash[h10 + iz0];
	int h010 = hash[h01 + iz0];
	int h110 = hash[h11 + iz0];
	int h001 = hash[h00 + iz1];
	int h101 = hash[h10 + iz1];
	int h011 = hash[h01 + iz1];
	int h111 = hash[h11 + iz1];

	float dtx = SmoothDerivative(tx);
	float dty = SmoothDerivative(ty);
	float dtz = SmoothDerivative(tz);
	tx = Smooth(tx);
	ty = Smooth(ty);
	tz = Smooth(tz);

	float a = h000;
	float b = h100 - h000;
	float c = h010 - h000;
	float d = h001 - h000;
	float e = h110 - h010 - h100 + h000;
	float f = h101 - h001 - h100 + h000;
	float g = h011 - h001 - h010 + h000;
	float h = h111 - h011 - h101 + h001 - h110 + h010 + h100 - h000;

	float4 sample;
	sample.x = a + b * tx + (c + e * tx) * ty + (d + f * tx + (g + h * tx) * ty) * tz;
	sample.y = (b + e * ty + (f + h * ty) * tz) * dtx;
	sample.z = (c + e * tx + (g + h * tx) * tz) * dty;
	sample.w = (d + f * tx + (g + h * tx) * ty) * dtz;
	sample.y *= frequency;
	sample.z *= frequency;
	sample.w *= frequency;
	return sample * (2.0 / hashMask) - 1.0;
}