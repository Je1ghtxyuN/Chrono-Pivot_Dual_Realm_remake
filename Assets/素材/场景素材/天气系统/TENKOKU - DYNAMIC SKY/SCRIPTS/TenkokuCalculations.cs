using System;
using UnityEngine;
using System.Collections;

namespace Tenkoku.Core
{

	[ExecuteInEditMode]
	
    public class TenkokuCalculations : MonoBehaviour
	{


	// NOTE:  All these calculations are based off of forumlas published by
	// Paul Schylter, in his paper 'computing planetary positions' which can
	// be viewed here: http://www.stjarnhimlen.se/comp/ppcomp.html
	// I've taken liberties with his procedure where I thought appropriate
	// or where I found it best suits the greater Tenkoku system.


	//PUBLIC VARIABLES
	[HideInInspector]
	public int dstOffset = 0;
	[HideInInspector]
	public int tzOffset = 0;
	[HideInInspector]
	public int yamt = 0;
	[HideInInspector]
	public float eclipticoffset = 0.0f;
	[HideInInspector]
	public float day = 0.0f; //day number
	[HideInInspector]
	public int y = 2000; //year
	[HideInInspector]
	public int m = 1; //month
	[HideInInspector]
	public int D = 1; //date?
	[HideInInspector]
	public float UT = 1.0f; //Universal Time
	[HideInInspector]
	public float LT = 1.0f; //Universal Time
	[HideInInspector]
	public float local_latitude;
	[HideInInspector]
	public float local_longitude;
	[HideInInspector]
	public float Epoch = 2000.0f;
	[HideInInspector]
	public int iteration = 1000;
	[HideInInspector]
	public float moonPos;
	[HideInInspector]
	public float azimuth;
	[HideInInspector]
	public float altitude;
	[HideInInspector]
	public float moonApogee = 1.0f;
	[HideInInspector]
	public float ecl;


	//PRIVATE VARIABLES
	private static float pi = 3.14159265359f;
	//private static float half_pi = 1.57079632679f;
	private static float _pi180 = (pi/180.0f);
	private static float _180pi = (180.0f/pi);

	private static float RADEG = (180.0f/pi);
	private static float DEGRAD = (pi/180.0f);

	private float useUT = 1.0f;
	private float use_latitude;
	private float use_longitude;

	private float N;
	private float i;
	private float w;
	private float a;
	private float e;
	private float M;

	private float v;
	private float E;

	private float xr;
	private float yr;
	private float zr;

	private float xhor;
	private float yhor;
	private float zhor;

	private float r;
	private float rs;
	private float lonsun;
	private float lonsunM;
	private float xs;
	private float ys;
	private float xe;
	private float ye;
	private float ze;

	private float RA;
	private float Dec;
	//private float rg;

	private float GMST0;
	private float Ls;
	private float LST;

	private float E0;
	private float E1;

	private float xg;
	private float yg;
	private float zg;
	    
	private float xh;
	private float yh;
	private float zh;

	private float lonecl;
	private float latecl;

	private float lon_corr;

	private float Mj;
	private float Mo;
	private float Mu;

	private float Ms;
	private float Mm;
	private float Nm;
	//private float ws;
	private float wm;
	private float Lm;
	private float Dm;
	private float F;

	private float LHA;

	private float lN;
	private float li;
	private float lw;
	private float la;
	private float le;
	private float lM;

	private float xv;
	private float yv;
	private int eL;
	private float ur;
	private int mA;
	//private float returnValue = 0f;




	public float GetElementData( int checkElement, int returnElement){

		//##### CALCULATE ORBITAL ELEMENTS #####
		//checkElements are decoded as:

		//sunstatic = 1
		//sun = 2
		//moon = 3
		//mercury = 4
		//venus = 5
		//mars = 6
		//jupiter = 7
		//saturn = 8
		//uranus = 9
		//neptune = 10

		//The primary orbital elements are here denoted as:
		
	    //N = 1. longitude of the ascending node
	    //i = 2. inclination to the ecliptic (plane of the Earth's orbit)
	    //w = 3. argument of perihelion
	    //a = 4. semi-major axis, or mean distance from Sun
	    //e = 5. eccentricity (0=circle, 0-1=ellipse, 1=parabola)
	    //M = 6. mean anomaly (0 at perihelion; increases uniformly with time)

		//Related orbital elements are:
	    //w1 = N + w   = longitude of perihelion
	    //L  = M + w1  = mean longitude
	    //q  = a*(1-e) = perihelion distance
	    //Q  = a*(1+e) = aphelion distance
	    //P  = a ^ 1.5 = orbital period (years if a is in AU, astronomical units)
	    //T  = Epoch_of_M - (M(deg)/360_deg) / P  = time of perihelion
	    //v  = true anomaly (angle between position and perihelion)
	    //E  = eccentric anomaly

		//Orbital elements of the Sun:
		if (checkElement == 1){
		    lN = 0.0f;
		    li = 0.0f;
		    lw = 282.9404f + 4.70935E-5f * day;
		    la = 1.000000f;  //(AU)
		    le = 0.016709f - 1.151E-9f * day;
		    lM = 356.0470f + 0.9856f * day;
		}
		
		if (checkElement == 2){
		    lN = 0.0f;
		    li = 0.0f;
		    lw = 282.9404f + 4.70935E-5f * day;
		    la = 1.000000f;  //(AU)
		    le = 0.016709f - 1.151E-9f * day;
		    lM = 356.0470f + 0.9856f * day;
		}
		
		//Orbital elements of the Moon:
		if (checkElement == 3){
			if (moonPos >= 0){
				day = 5574f + ((m-4)*30f) - (27.31f*(moonPos/360));
			}
		    lN = 125.1228f - 0.0529538083f * day;
		    li = 5.1454f;
		    lw = 318.0634f + 0.1643573223f * day;
		    la = 60.2666f;  //(Earth radii)
		    le = 0.054900f;
		    lM = 115.3654f + 13.0649929509f * day;
		}
		
		//Orbital elements of Mercury:
		if (checkElement == 4){
		    lN =  48.3313f + 3.24587E-5f * day;
		    li = 7.0047f + 5.00E-8f * day;
		    lw =  29.1241f + 1.01444E-5f * day;
		    la = 0.387098f;  //(AU)
		    le = 0.205635f + 5.59E-10f * day;
		    lM = 168.6562f + 4.0923344368f * day;
		}
		
		//Orbital elements of Venus:
		if (checkElement == 5){
		    lN =  76.6799f + 2.46590E-5f * day;
		    li = 3.3946f + 2.75E-8f * day;
		    lw =  54.8910f + 1.38374E-5f * day;
		    la = 0.723330f;  //(AU)
		    le = 0.006773f - 1.302E-9f * day;
		    lM = 48.0052f + 1.6021302244f * day;
		}
		
		//Orbital elements of Mars:
		if (checkElement == 6){
		    lN =  49.5574f + 2.11081E-5f * day;
		    li = 1.8497f - 1.78E-8f * day;
		    lw = 286.5016f + 2.92961E-5f * day;
		    la = 1.523688f; //(AU)
		    le = 0.093405f + 2.516E-9f * day;
		    lM = 18.6021f + 0.5240207766f * day;
		}
		
		//Orbital elements of Jupiter:
		if (checkElement == 7){
		    lN = 100.4542f + 2.76854E-5f * day;
		    li = 1.3030f - 1.557E-7f * day;
		    lw = 273.8777f + 1.64505E-5f * day;
		    la = 5.20256f;  //(AU)
		    le = 0.048498f + 4.469E-9f * day;
		    lM =  19.8950f + 0.0830853001f * day;
		}
		
		//Orbital elements of Saturn:
		if (checkElement == 8){
		    lN = 113.6634f + 2.38980E-5f * day;
		    li = 2.4886f - 1.081E-7f * day;
		    lw = 339.3939f + 2.97661E-5f * day;
		    la = 9.55475f;  //(AU)
		    le = 0.055546f - 9.499E-9f * day;
		    lM = 316.9670f + 0.0334442282f * day;
		}
		
		//Orbital elements of Uranus:
		if (checkElement == 9){
		    lN =  74.0005f + 1.3978E-5f * day;
		    li = 0.7733f + 1.9E-8f * day;
		    lw =  96.6612f + 3.0565E-5f * day;
		    la = 19.18171f - 1.55E-8f * day;  //(AU)
		    le = 0.047318f + 7.45E-9f * day;
		    lM = 142.5905f + 0.011725806f * day;
		}
		
		//Orbital elements of Neptune:
		if (checkElement == 10){
		    lN = 131.7806f + 3.0173E-5f * day;
		    li = 1.7700f - 2.55E-7f * day;
		    lw = 272.8461f - 6.027E-6f * day;
		    la = 30.05826f + 3.313E-8f * day;  //(AU)
		    le = 0.008606f + 2.15E-9f * day;
		    lM = 260.2471f + 0.005995147f * day;
		}
		
		float returnValue = 0f;

		if (returnElement == 1) returnValue = lN;
		if (returnElement == 2) returnValue = li;
		if (returnElement == 3) returnValue = lw;
		if (returnElement == 4) returnValue = la;
		if (returnElement == 5) returnValue = le;
		if (returnElement == 6) returnValue = lM;

		return returnValue;
	}




	//function CalculateNode(checkElement : String) : String {
	public void CalculateNode(int checkElement){

		//##### CALCULATE TIME SCALE #####
		// d = day number
		// y = year
		// m = month
		// D = date
		// UT = UT in hours+decimals
		
		useUT = UT - tzOffset + dstOffset;
		use_latitude = local_latitude;
		use_longitude = local_longitude;
		day = 367 * y - 7 * (y+(m+9)/12) / 4 + 275 * m / 9 + D - 730530;
		day += useUT/24.0f;
		
	  	ecl = 23.4393f - 3.563E-7f * day;

		N = GetElementData(checkElement, 1);
		i = GetElementData(checkElement, 2);
		w = GetElementData(checkElement, 3);
		a = GetElementData(checkElement, 4);
		e = GetElementData(checkElement, 5);
		M = GetElementData(checkElement, 6);

		//solve M
		M = Rev(M);

		//##### CALCULATE SUN POSITION #####
		if (checkElement == 2 || checkElement == 1){
		
			//M -= 0.0006; //fudge!
			
			// First, compute the eccentric anomaly E from the mean anomaly M and from
			// the eccentricity e (E and M in degrees):
			E = M + e*_180pi * Sind(M) * ( 1 + e * Cosd(M) );

			// Then compute the Sun's distance r and its true anomaly v from:
		    xv = Cosd(E) - e;
		    yv = (Mathf.Sqrt(1 - (e*e)) * Sind(E));

		    r = Mathf.Sqrt( xv*xv + yv*yv );
		    v = (Mathf.Atan2(yv,xv)*_180pi);
			rs = r;
			
		    // compute the Sun's true longitude:
		    lonsun = (v + w);
		    
		    // compute the Sun's mean longitude:
		    lonsunM = (w + M);

			lonsun = Rev(lonsun);
			lonsunM = Rev(lonsunM);

			// Convert lonsun,r to ecliptic rectangular geocentric coordinates xs,ys:
	    	xs = r * Cosd(lonsun);
	    	ys = r * Sind(lonsun);

			// (since the Sun always is in the ecliptic plane, zs is of course zero).
			// xs,ys is the Sun's position in a coordinate system in the plane of the ecliptic.
			// To convert this to equatorial, rectangular, geocentric coordinates, compute:
		    xe = xs;
		    ye = ys * Cosd(ecl);
		    ze = ys * Sind(ecl);

			// Finally, compute the Sun's Right Ascension (RA) and Declination (Dec):
		    RA  = Mathf.Atan2(ye,xe);
		    Dec = Mathf.Atan2(ze,Mathf.Sqrt(xe*xe+ye*ye));
		    

			//##### SIDEREAL TIME #####
			//We need the Sun's mean longitude, Ls, which can be computed from the Sun's v and w as follows: 
			//Ls = (v + w);
			// this is lonsun as calculated above
			
			// The GMST0 is easily computed from Ls (divide by 15 if you want GMST0 in hours rather
			// than degrees), GMST is then computed by adding the UT, and finally the LST is computed
			// by adding your local longitude (east longitude is positive, west negative).

			// Note that "time" is given in hours while "angle" is given in degrees. The two are related
			// to one another due to the Earth's rotation: one hour is here the same as 15 degrees.
			// Before adding or subtracting a "time" and an "angle", be sure to convert them to the same
			// unit, e.g. degrees by multiplying the hours by 15 before adding/subtracting:
		    GMST0 = (lonsunM/15.0f)+12.0f;
			LST = GMST0 + useUT + (use_longitude/15.0f);
			
			if (LST > 24){
				LST -= 24;
			} else if (LST < 0){
				LST += 24;
			}
			
			LT = LST;
				
			// The formulae above are written as if times are expressed in degrees. If we instead
			// assume times are given in hours and angles in degrees, and if we explicitly write
			// out the conversion factor of 15, we get:
		    //GMST0 = 15 * (Ls + 180.0);
		    //GMST = GMST0 + UT;
		    //LST  = GMST + (local_longitude/15);

		
			//##### COMPUTE SUN'S HOUR ANGLE #####
			LHA = LST - ((RA*RADEG)/15.0f);

			// Convert to Rectangular Coordinates
			xr = Cosd(LHA*15.0f) * Mathf.Cos(Dec);
		    yr = Sind(LHA*15.0f) * Mathf.Cos(Dec);
		    zr = Mathf.Sin(Dec);
			
			// Rotate Coordinates
			xhor = xr * Sind(use_latitude) - zr * Cosd(use_latitude);
	   	 	yhor = yr;
	    	zhor = xr * Cosd(use_latitude) + zr * Sind(use_latitude);
	    
			//Compute azimuth and altitude
	   	 	azimuth  = Mathf.Atan2(yhor,xhor)*RADEG + 180.0f;
	   		altitude = Asind(zhor);	
			


		} else {

			//Solve N
			N = Rev(N);

		
			//##### POSITION OF THE MOON AND PLANETS #####
			// First, compute the eccentric anomaly, E, from M, the mean anomaly, and e, the eccentricity.
			// As a first approximation, do (E and M in degrees):
			E = M + e*_180pi * Sind(M) * ( 1 + e * Cosd(M) );
			
			// If e, the eccentricity, is less than about 0.05-0.06, this approximation is sufficiently accurate.
			// If the eccentricity is larger, set E0=E and then use this iteration formula (E and M in degrees):
			//if (e > 0.05){
				E0 = E;
		    	// For each new iteration, replace E0 with E1. Iterate until E0 and E1 are sufficiently close together
				// (about 0.001 degrees). For comet orbits with eccentricites close to one, a difference of less than
				// 1E-4 or 1E-5 degrees should be required.

				// If this iteration formula won't converge, the eccentricity is probably too close to one. Then you
				// should instead use the formulae for near-parabolic or parabolic orbits.
				for (eL = 0; eL < iteration; eL++){
			   		E1 = E0 - (E0 - _180pi * e * Sind(E0) - M) / ( 1.0f - e * Cosd(E0));

					//if (Mathf.Approximately(E0,E1)){
					if (Mathf.Abs(E1)-Mathf.Abs(E0) < 0.005f){
						break;
					} else {
						E0 = E1;
					}
				}
				E = E1;
			//}
			
			// Now compute the planet's distance and true anomaly:
		    xv = a * (Cosd(E) - e);
		    yv = a * (Mathf.Sqrt(1.0f - e*e) * Sind(E));
		    r = Mathf.Sqrt(xv*xv+yv*yv);
		    v = Atan2(yv,xv)*RADEG;
			v = Rev(v);


			//##### THE POSITION IN SPACE #####
			//Compute the planet's position in 3-dimensional space:
		    xh = r * (Cosd(N) * Cosd(v+w) - Sind(N) * Sind(v+w) * Cosd(i));
		    yh = r * (Sind(N) * Cosd(v+w) + Cosd(N) * Sind(v+w) * Cosd(i));
		    zh = r * (Sind(v+w) * Sind(i));

			// For the Moon, this is the geocentric (Earth-centered) position in the ecliptic
			// coordinate system. For the planets, this is the heliocentric (Sun-centered) position,
			// also in the ecliptic coordinate system. If one wishes, one can compute the ecliptic
			// longitude and latitude (this must be done if one wishes to correct for perturbations,
			// or if one wants to precess the position to a standard epoch):
		    lonecl = Atan2(yh,xh)*RADEG;
		    latecl = Atan2(zh,Mathf.Sqrt(xh*xh+yh*yh))*RADEG;
		    
		    
			// As a check one can compute sqrt(xh*xh+yh*yh+zh*zh), which of course should equal r
			// (except for small round-off errors).

		    
			//##### PRECESSION #####
			// If one wishes to compute the planet's position for some standard epoch, such as
			// 1950.0 or 2000.0 (e.g. to be able to plot the position on a star atlas), one must add
			// the correction below to lonecl. If a planet's and not the Moon's position is computed,
			// one must also add the same correction to lonsun, the Sun's longitude. The desired Epoch
			// is expressed as the year, possibly with a fraction.
			//Epoch = y;
			lon_corr = 3.82394E-5f * (365.2422f * (Epoch - 2000.0f) - day);


			lonecl = Rev(lonecl);

			// If one wishes the position for today's epoch (useful when computing rising/setting times
			// and the like), no corrections need to be done.


			//##### PERTURBATIONS OF THE MOON #####
			if (checkElement == 3){
			// If the position of the Moon is computed, and one wishes a better accuracy than about 2 degrees,
			// the most important perturbations has to be taken into account. If one wishes 2 arc minute accuracy,
			// all the following terms should be accounted for. If less accuracy is needed, some of the smaller
			// terms can be omitted.
			

				//First compute data:
				Ms = Rev(GetElementData(2, 6));	// Mean Anomaly of the Sun
				Mm = Rev(GetElementData(3, 6));	// Mean Anomaly of the Moon
				Nm = Rev(GetElementData(3, 1));	// Longitude of the Moon's node
				//ws = Rev(GetElementData(2, 3));	// Argument of perihelion for the Sun
				wm = Rev(GetElementData(3, 3));	// Argument of perihelion for the Moon
				
				Ls = lonsunM;										// Mean Longitude of the Sun  (Ns=0)
				Lm = Rev(Mm + wm + Nm);								// Mean longitude of the Moon
				Dm = Rev(Lm - Ls);									// Mean elongation of the Moon
				F = Rev(Lm - Nm);									// Argument of latitude for the Moon



				//Add these terms to the Moon's longitude (degrees):
			    lonecl -= 1.274f * Sind(Mm - (2.0f*Dm));          		// (the Evection)
			    lonecl += 0.658f * Sind(2.0f*Dm);               		// (the Variation)
			    lonecl -= 0.186f * Sind(Ms);                 		// (the Yearly Equation)
			    lonecl -= 0.059f * Sind((2.0f*Mm) - (2.0f*Dm));
			    lonecl -= 0.057f * Sind(Mm - (2.0f*Dm) + Ms);
			    lonecl += 0.053f * Sind(Mm + (2.0f*Dm));
			    lonecl += 0.046f * Sind((2.0f*Dm) - Ms);
			    lonecl += 0.041f * Sind(Mm - Ms);
			    lonecl -= 0.035f * Sind(Dm);                 		// (the Parallactic Equation)
			    lonecl -= 0.031f * Sind(Mm + Ms);
			    lonecl -= 0.015f * Sind((2.0f*F) - (2.0f*Dm));
			    lonecl += 0.011f * Sind(Mm - (4.0f*Dm));

				//Add these terms to the Moon's latitude (degrees):
			    latecl -= 0.173f * Sind(F - (2.0f*Dm));
			    latecl -= 0.055f * Sind((Mm) - F - (2.0f*Dm));
			    latecl -= 0.046f * Sind((Mm) + F - (2.0f*Dm));
			    latecl += 0.033f * Sind(F + (2.0f*Dm));
			    latecl += 0.017f * Sind((2.0f*Mm) + F);

				// All perturbation terms that are smaller than 0.01 degrees in longitude or latitude and
				// smaller than 0.1 Earth radii in distance have been omitted here. A few of the largest
				// perturbation terms even have their own names! The Evection (the largest perturbation) was
				// discovered already by Ptolemy a few thousand years ago (the Evection was one of Ptolemy's epicycles).
				// The Variation and the Yearly Equation were both discovered by Tycho Brahe in the 16'th century.

				// The computations can be simplified by omitting the smaller perturbation terms. The error introduced
				// by this seldom exceeds the sum of the amplitudes of the 4-5 largest omitted terms. If one only
				// computes the three largest perturbation terms in longitude and the largest term in latitude, the
				// error in longitude will rarley exceed 0.25 degrees, and in latitude 0.15 degrees.
			}


			//##### PERTURBATIONS OF JUPITER, SATURN, and URANUS #####
			if (checkElement == 7 || checkElement == 8 || checkElement == 9){
			// The only planets having perturbations larger than 0.01 degrees are Jupiter, Saturn and Uranus.
			// First compute:
		    // Mj    Mean anomaly of Jupiter
		    // Mo    Mean anomaly of Saturn
		    // Mu    Mean anomaly of Uranus (needed for Uranus only)
			Mj = GetElementData(7, 6);
			Mo = GetElementData(8, 6);
			Mu = GetElementData(9, 6);
			
			//Perturbations for Jupiter. Add these terms to the longitude:
			if (checkElement == 7){
			    lonecl -= 0.332f * Mathf.Sin(2*Mj - 5*Mo - 67.6f);
			    lonecl -= 0.056f * Mathf.Sin(2*Mj - 2*Mo + 21.0f);
			    lonecl += 0.042f * Mathf.Sin(3*Mj - 5*Mo + 21.0f);
			    lonecl -= 0.036f * Mathf.Sin(Mj - 2*Mo);
			    lonecl += 0.022f * Mathf.Cos(Mj - Mo);
			    lonecl += 0.023f * Mathf.Sin(2*Mj - 3*Mo + 52.0f);
			    lonecl -= 0.016f * Mathf.Sin(Mj - 5*Mo - 69.0f);
			}
			
			//Perturbations for Saturn. Add these terms to the longitude:
			if (checkElement == 8){
			    lonecl += 0.812f * Mathf.Sin(2*Mj - 5*Mo - 67.6f);
			    lonecl -= 0.229f * Mathf.Cos(2*Mj - 4*Mo - 2.0f);
			    lonecl += 0.119f * Mathf.Sin(Mj - 2*Mo - 3.0f);
			    lonecl += 0.046f * Mathf.Sin(2*Mj - 6*Mo - 69.0f);
			    lonecl += 0.014f * Mathf.Sin(Mj - 3*Mo + 32.0f);

				//For Saturn: also add these terms to the latitude:
			    latecl -= 0.020f * Mathf.Cos(2*Mj - 4*Mo - 2.0f);
			    latecl += 0.018f * Mathf.Sin(2*Mj - 6*Mo - 49.0f);
			}

			//Perturbations for Uranus: Add these terms to the longitude:
			if (checkElement == 9){
			    lonecl += 0.040f * Mathf.Sin(Mo - 2*Mu + 6.0f);
			    lonecl += 0.035f * Mathf.Sin(Mo - 3*Mu + 33.0f);
			    lonecl -= 0.015f * Mathf.Sin(Mj - Mu + 20.0f);
			}

			// The "great Jupiter-Saturn term" is the largest perturbation for both Jupiter and Saturn.
			// Its period is 918 years, and its amplitude is 0.332 degrees for Jupiter and 0.812 degrees
			// for Saturn. These is also a "great Saturn-Uranus term", period 560 years, amplitude 0.035
			// degrees for Uranus, less than 0.01 degrees for Saturn (and therefore omitted). The other
			// perturbations have periods between 14 and 100 years. One should also mention the
			// "great Uranus-Neptune term", which has a period of 4220 years and an amplitude of about
			// one degree. It is not included here, instead it is included in the orbital elements of
			// Uranus and Neptune.

			// For Mercury, Venus and Mars we can ignore all perturbations. For Neptune the only significant
			// perturbation is already included in the orbital elements, as mentioned above, and therefore no
			// further perturbation terms need to be accounted for.
			
			}


			//##### GEOCENTRIC (EARTH-CENTERED) COORDINATES #####
			// Now we have computed the heliocentric (Sun-centered) coordinate of the planet, and we have
			// included the most important perturbations. We want to compute the geocentric (Earth-centerd)
			// position. We should convert the perturbed lonecl, latecl, r to (perturbed) xh, yh, zh:
			lonecl += lon_corr;
			lonsun += lon_corr;
			ur = r;
			if (checkElement == 3) ur = 1.0f;
		    xh = ur * Cosd(lonecl) * Cosd(latecl);
		    yh = ur * Sind(lonecl) * Cosd(latecl);
		    zh = ur * Sind(latecl);

			// If we are computing the Moon's position, this is already the geocentric position, and thus we
			// simply set xg=xh, yg=yh, zg=zh. Otherwise we must also compute the Sun's position:
			// convert lonsun, rs (where rs is the r computed here) to xs, ys:
		    xs = rs * Cosd(lonsun);
		    ys = rs * Sind(lonsun);
		    
			if (checkElement == 3){
			    xs = 0.0f;
		    	ys = 0.0f;
			}

			// (Of course, any correction for precession should be added to lonecl and lonsun before
			// converting to xh,yh,zh and xs,ys).

			// Now convert from heliocentric to geocentric position:
		    xg = xh + xs;
		    yg = yh + ys;
		    zg = zh;

			//We now have the planet's geocentric (Earth centered) position in rectangular, ecliptic coordinates.



			//##### EQUATORIAL COORDINATES #####
			// Let's convert our rectangular, ecliptic coordinates to rectangular, equatorial coordinates:
			// simply rotate the y-z-plane by ecl, the angle of the obliquity of the ecliptic:
		    xe = xg;
		    ye = yg * Cosd(ecl) - zg * Sind(ecl);
		    ze = yg * Sind(ecl) + zg * Cosd(ecl);

			// Finally, compute the planet's Right Ascension (RA) and Declination (Dec):
		    RA  = Atan2(ye,xe);
		    Dec = Atan2(ze,Mathf.Sqrt(xe*xe+ye*ye));

			// Compute the geocentric distance:
		    //rg = Mathf.Sqrt(xe*xe+ye*ye+ze*ze); // or use Mathf.Sqrt(xg*xg+yg*yg+zg*zg)
		    

			if (checkElement == 3){
			//Add these terms to the Moon's distance (Earth radii):
			    r -= 0.58f * Cosd((Mm) - (2.0f*Dm*RADEG));
			    r -= 0.46f * Cosd(2.0f*Dm);
			}
			
			//This completes our computation of the equatorial coordinates.





			/*
			//##### MOON TOPOCENTRIC POSITION #####
			if (checkElement == "moon"){
				// The Moon's position, as computed earlier, is geocentric, i.e. as seen by an
				// imaginary observer at the center of the Earth. Real observers dwell on the surface
				// of the Earth, though, and they will see a different position - the topocentric position.
				// This position can differ by more than one degree from the geocentric position. To compute
				// the topocentric positions, we must add a correction to the geocentric position.

				// Let's start by computing the Moon's parallax, i.e. the apparent size of the (equatorial)
				// radius of the Earth, as seen from the Moon:
		    	mpar = Mathf.Asin(1.0/r);

				// where r is the Moon's distance in Earth radii. It's simplest to apply the correction
				// in horizontal coordinates (azimuth and altitude): within our accuracy aim of 1-2 arc minutes,
				// no correction need to be applied to the azimuth. One need only apply a correction to the
				// altitude above the horizon:
		    	//alt_topoc = alt_geoc - mpar * Mathf.Cos(alt_geoc);  <-- not sure if necessary,

				// Sometimes one need to correct for topocentric position directly in equatorial coordinates
				// though, e.g. if one wants to draw on a star map how the Moon passes in front of the Pleiades,
				// as seen from some specific location. Then we need to know the Moon's geocentric Right Ascension
				// and Declination (RA, Decl), the Local Sidereal Time (LST), and our latitude (lat).

				// Our astronomical latitude (lat) must first be converted to a geocentric latitude (gclat), and
				// distance from the center of the Earth (rho) in Earth equatorial radii. If we only want an
				// approximate topocentric position, it's simplest to pretend that the Earth is a perfect sphere,
				// and simply set:
		    	//gclat = lat,  rho = 1.0  <--- nope, let's go all the way :)

				//However, if we do wish to account for the flattening of the Earth, we instead compute:
		    	gclat = use_latitude - 0.1924 * Sind(2.0*use_latitude);
		    	rho   = 0.99833 + 0.00167 * Cosd(2.0*use_latitude);

				// Next we compute the Moon's geocentric Hour Angle (HA) from the Moon's geocentric RA.
				// First we must compute LST as described in 5b above, then we compute HA as:
		    	//HA = LST - RA;
				HA = (LST*15.0) - ((RA * RADEG));
				
				//We also need an auxiliary angle, g:
		   		g = Mathf.Atan(Tand(gclat) / Cosd(HA));

				// Now we're ready to convert the geocentric Right Ascention and Declination (RA, Dec)
				// to their topocentric values (topRA, topDecl):
		    	topRA   = RA  - mpar * rho * Cosd(gclat) * Sind(HA) / Cosd(Dec);
		    	
		    	
		    	
		    	if (gclat != 0.0){
		   	 		topDecl = Dec - (mpar * rho * Sind(gclat) * Sind((g) - (Dec*RADEG)) / Sind(g));
				} else {
					// (Note that if decl is exactly 90 deg, cos(Dec) becomes zero and we get a division by zero
					// when computing topRA, but that formula breaks down only very close to the celestial poles
					// anyway and we never see the Moon there. Also if gclat is precisely zero, g becomes zero too,
					// and we get a division by zero when computing topDecl. In that case, replace the formula for
					// topDecl with:
		    		topDecl = Dec - (mpar * rho * Sind(-Dec) * Cosd(HA));
				}
				// which is valid for gclat equal to zero; it can also be used for gclat extremely close to zero).

				//This correction to topocentric position can also be applied to the Sun and the planets.
				// But since they're much farther away, the correction becomes much smaller. It's largest for Venus
				// at inferior conjunction, when Venus' parallax is somewhat larger than 32 arc seconds. Within our
				// aim of obtaining a final accuracy of 1-2 arc minutes, it might barely be justified to correct
				// to topocentric position when Venus is close to inferior conjunction, and perhaps also when Mars
				// is at a favourable opposition. But in all other cases this correction can safely be ignored
				// within our accuracy aim. We only need to worry about the Moon in this case.

				// If you want to compute topocentric coordinates for the planets too, you do it the same way as
				// for the Moon, with one exception: the Moon's parallax is replaced by the parallax of the planet
				// (ppar), as computed from this formula:
		    	//ppar = (8.794/3600)_deg / r  <--- we don't need this, I've commented it out.

				// where r is the distance of the planet from the Earth, in astronomical units.
				
				//Finally reassign position data
				RA = topRA;
				Dec = topDecl;
				
				
				//assign specific variables
				moonApogee = (r-57.517902)/5.97789;	
			}
			*/

			//##### POSITION OF PLUTO #####
			// wait, wait wait!  What about Pluto?  Despite the recent controversial declassification
			// of Pluto from being a "planet" planet, to being a "dwarf-planet" planet, that has nothing to do
			// with the reasoning of why it isn't included here in these calculations!
			
			// It simply is not visible to the human eye, and therefore I found it unnecessary to include in Tenkoku.
			// Also the calculations for the positioning of Pluto have never been accurately described, as the other
			// planets have been.  What exists now is simply an analytic "hedge" and loses a lot of accuracy over
			// a few hundred years.  All of this is a moot point though, since it isn't actually visible.

			
		}
		


		
		//##### COMPUTE  HOUR ANGLE #####
		LHA = LST - ((RA*RADEG)/15.0f);

		// Convert to Rectangular Coordinates
		xr = Cosd(LHA*15.0f) * Mathf.Cos(Dec);
	    yr = Sind(LHA*15.0f) * Mathf.Cos(Dec);
	    zr = Mathf.Sin(Dec);
		
		// Rotate Coordinates
		xhor = xr * Sind(use_latitude) - zr * Cosd(use_latitude);
		yhor = yr;
		zhor = xr * Cosd(use_latitude) + zr * Sind(use_latitude);

		//Compute azimuth and altitude
		azimuth  = Mathf.Atan2(yhor,xhor)*RADEG + 180.0f;
		altitude = Asind(zhor);	

	}











	public float Sind (float x){
		return Mathf.Sin((x)*DEGRAD);
	}
	public float Cosd (float x){
		return Mathf.Cos((x)*DEGRAD);
	}
	public float Tand (float x){
		return Mathf.Tan((x)*DEGRAD);
	}
	public float Asind (float x){
		return (RADEG*Mathf.Asin(x));
	}
	public float Acosd (float x){
		return (RADEG*Mathf.Acos(x));
	}
	public float Atand (float x) {
		return (RADEG*Mathf.Atan(x));
	}
	public float Atan2d (float x, float y) {
		return (RADEG*Mathf.Atan2((y),(x)));
	}


	public float Rev( float x){
	    return  x - Mathf.Floor(x/360.0f)*360.0f;
	}


	public float Cbrt( float x ) {

	    if ( x > 0.0f ){
	        x = Mathf.Exp( Mathf.Log(x) / 3.0f );
	    } else if ( x < 0.0f ){
	        x = -Mathf.Pow(-x,(1.0f/3.0f));
	    } else {
	        x = 0.0f;
	    }
	    return x;

	}



	public float Atan2( float y, float x ) {

	    if (x > 0.0f){
	    	x = Mathf.Atan(y/x);             
	    } else if (x < 0.0f){
	    	x = Mathf.Atan(y/x) + (180.0f*_pi180);
	    }else if (x == 0.0f){
	    	x = Mathf.Sign(y) * (90.0f*_pi180);   
		}

	    return x;
	}



	public float SolveAngle( float deg ) {

		for (mA = 0; mA < iteration; mA++){
			if (deg < 0.0f){
				deg += 360.0f;
			} else {
				break;
			}
		}
		for (mA = 0; mA < iteration; mA++){
			if (deg > 360.0f){
				deg -= 360.0f;
			} else {
				break;
			}
		}
		
		return deg;
	}



	    
	    
}
}