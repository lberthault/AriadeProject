/* An error represents a bad decision at a specific starting intersection */
public class Error
{
    /* The path the participant has to follow to correct the error */
    public Path path;
    public bool isCorrected;
    /* Start and end time */
    private float start, end;
    /* The number of times the participant took bad decisions in this error i.e. not turning back or not following the right arrows */
    private int numberOfWrongAreas;
    
    public Error(Area from, Area to, float simTime)
    {
        start = simTime;
        path = new Path();
        path.Add(from);
        path.Add(to);
        numberOfWrongAreas++;
        isCorrected = false;
    }

    /* If the error is not corrected, this is called each time the participant passes through an area and updates the variables */
    public bool Update(Area area)
    {
        if (!path.Contains(area))
        {
            path.Add(area);
            numberOfWrongAreas++;
            return true;
        } else
        {
            path.RemoveLast();
            return false;
        }

    }

    public void SetCorrect(float simTime)
    {
        end = simTime;
        isCorrected = true;
    }

    public int NumberOfWrongAreas()
    {
        // Doesn't count the start/end area
        return numberOfWrongAreas;
    }

    public float Time()
    {
        return end - start;
    }
}