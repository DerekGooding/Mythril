namespace Mythril.GameLogic.Materia;

public static class MateriaLogic
{
    public static void AddAP(this Data.Materia.Materia materia, int amount)
    {
        if (materia.Level >= materia.MaxLevel)
        {
            materia.AP = System.Math.Min(materia.AP + amount, materia.MaxAP);
            return;
        }

        materia.AP += amount;
        while (materia.AP >= materia.MaxAP && materia.Level < materia.MaxLevel)
        {
            materia.AP -= materia.MaxAP;
            materia.Level++;
        }

        if (materia.Level >= materia.MaxLevel)
        {
            materia.AP = System.Math.Min(materia.AP, materia.MaxAP);
        }
    }
}
