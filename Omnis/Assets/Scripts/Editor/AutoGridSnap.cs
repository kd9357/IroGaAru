using UnityEngine;
using UnityEditor;
 
// While this window is open editor transform movement/rotation will auto snap to grid
public class AutoGridSnap : EditorWindow
{
	// Vars
    private Vector3 prevPosition;
	private Vector3 prevRotation;
    private bool doSnap = true;
    private float snapValueX = 1;
	private float snapValueY = 1;
	private float snapValueZ = 1;
	private float snapValueRot = 45;
	
    [MenuItem( "Edit/Auto Snap %_l" )]
     
    static void Init()
    {
		// Create window
	    AutoGridSnap window = (AutoGridSnap)EditorWindow.GetWindow( typeof( AutoGridSnap ) );
	    window.maxSize = new Vector2( 300, 200 );
		
		// Fetch editor values
		window.snapValueX = EditorPrefs.GetFloat("MoveSnapX", 1.0f);
		window.snapValueY = EditorPrefs.GetFloat("MoveSnapY", 1.0f);
		window.snapValueZ = EditorPrefs.GetFloat("MoveSnapZ", 1.0f);
		window.snapValueRot = EditorPrefs.GetFloat("RotationSnap", 45.0f);
    }
     
    public void OnGUI()
    {
		// Allow for on the fly value changes
	    doSnap = EditorGUILayout.Toggle( "Auto Snap", doSnap );
	    snapValueX = EditorGUILayout.FloatField( "X Snap Value", snapValueX );
		snapValueY = EditorGUILayout.FloatField( "Y Snap Value", snapValueY );
		snapValueZ = EditorGUILayout.FloatField( "Z Snap Value", snapValueZ );
		snapValueRot = EditorGUILayout.FloatField( "Rotate Snap", snapValueRot );
		
		// Update the actual unity editor values
		EditorPrefs.SetFloat("MoveSnapX", snapValueX);
		EditorPrefs.SetFloat("MoveSnapY", snapValueY);
		EditorPrefs.SetFloat("MoveSnapZ", snapValueZ);
		EditorPrefs.SetFloat("RotationSnap", snapValueRot);
	}
     
    public void Update()
    {
		// Check if we should snap
	    if ( doSnap
	    && !EditorApplication.isPlaying
	    && Selection.transforms.Length > 0
	    && (Selection.transforms[0].position != prevPosition || Selection.transforms[0].eulerAngles != prevRotation) )
	    {
		    AutoSnap();
		    prevPosition = Selection.transforms[0].position;
			prevRotation = Selection.transforms[0].eulerAngles;
	    }
    }
     
    private void AutoSnap()
    {
		// Snap the transforms
    	foreach ( Transform transform in Selection.transforms )
	    {
		    Vector3 t = transform.transform.position;
		    t.x = SnapRound( t.x, snapValueX );
		    t.y = SnapRound( t.y, snapValueY );
		    t.z = SnapRound( t.z, snapValueZ );
		    transform.transform.position = t;
			
			Vector3 r = transform.transform.eulerAngles;
			r.x = SnapRound( r.x, snapValueRot ) % 360.0f;
		    r.y = SnapRound( r.y, snapValueRot ) % 360.0f;
		    r.z = SnapRound( r.z, snapValueRot ) % 360.0f;
			transform.transform.eulerAngles = r;
	    }
    }
	
    private float SnapRound( float input, float snapValue )
    {
    	return snapValue * Mathf.Round( ( input / snapValue ) );
    }
}