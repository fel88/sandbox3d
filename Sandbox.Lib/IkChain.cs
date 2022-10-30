using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenTK;

namespace Sandbox.Lib
{
    public class IkChain
    {
        public IkChain()
        {
            Settings = new IkSettings();
            DeMask = new float[6] { 1, 1, 1, 1, 1, 1 };
        }
        public IEntityContainer Container { get; set; }

        public string Name { get; set; }
        public IkLineBone EndEffector { get; set; }
        public IkRevoluteJoint[] Joints { get; set; }
        public IkSettings Settings { get; set; }
        public bool Enable { get; set; }
        public Vector3 Offset { get; set; }
        
        public float[] DeMask { get; set; }

        
        public virtual bool Solve(float epsilon = 0.01f, int maxIter = 1000)
        {            

            var orig = EndEffector.Parent.Parent.GetPose();
            var sk = EndEffector.Parent.Parent;
            
            bool good = false;
            float sum1 = 0;
            float sum2 = 0;
            float min = float.MaxValue;
            for (int it = 0; it < maxIter; it++)
            {

                var mt1 = Stuff.MeasureTime(() => { sk.ForwardCalc(Matrix3.Identity); });
                sum1 += mt1;
                float[,] de = null;
                var mt2 = Stuff.MeasureTime(() => { de = Update(); });
                sum2 += mt2;

                float er = 0;
                for (int i = 0; i < de.GetLength(0); i++)
                {
                    for (int j = 0; j < de.GetLength(1); j++)
                    {
                        er += Math.Abs(de[i, j]);
                    }
                }
                
                if (er < min)
                {
                    min = er;
                }
                if (er < epsilon)
                {
                    good = true;
                    break;
                }
            }
            if (!good)
            {
                sk.SetPose(orig);
            }
            return good;
        }

        public Vector3 LastPositionError { get; set; }
        public Vector3 LastEulerError { get; set; }
        
        public virtual float[,] Update()
        {
            #region ik update

            var ikSettings = Settings;
            float ikDelta = ikSettings.Delta;
            float ikDelta2 = ikSettings.OrientationDelta;

            Matrix<double> de = null;

            var com = Container.GetCenterMass();

            if (Enable)
            {
                if (EndEffector != null && Joints != null )
                {
                    Matrix<double> dlo = null;
                    Matrix<double> jac = null;

                    
                    EndEffector.Parent.Orientation.Normalize();

                    var offsetVector = new OpenTK.Vector4(Offset) * EndEffector.Parent.CoordSystem;
                    var target = ikSettings.TargetPosition + offsetVector.Xyz;
                    if (ikSettings.IsRelative)
                    {
                        //target -= Settings.PivotBone.AbsolutePosition;
                    }

                    switch (ikSettings.Type)
                    {
                        case IkTypeEnum.Full6D:
                            {
                                //var jac = CalcJacobian(Stuff.Registers.EndEffector, Stuff.Registers.IkJoints, Stuff.ToVector3(ps));
                                jac = Stuff.ToMathNetMatrix(IkCalculator.CalcJacobianOrientation(Joints, Stuff.ToVector3(target)));
                                //transpose jac

                                de = new DenseMatrix(6, 1);

                                de[0, 0] = (target.X - EndEffector.AbsolutePosition.X) * ikDelta;
                                de[1, 0] = (target.Y - EndEffector.AbsolutePosition.Y) * ikDelta;
                                de[2, 0] = (target.Z - EndEffector.AbsolutePosition.Z) * ikDelta;

                                LastPositionError = target - EndEffector.AbsolutePosition;

                                //de[3, 0] = (q.X - Stuff.Registers.EndEffector.Parent.Orientation.X);
                                //de[4, 0] = (q.Y - Stuff.Registers.EndEffector.Parent.Orientation.Y);
                                //de[5, 0] = (q.Z - Stuff.Registers.EndEffector.Parent.Orientation.Z);
                                var q = ikSettings.TargetOrientation;
                                q.Normalize();
                                var err = q * EndEffector.Parent.Orientation;
                                EndEffector.Parent.Orientation.Normalize();

                                //var euler1 = Stuff.ToEulerianAngle(Stuff.Registers.EndEffector.Parent.Orientation);
                                //var euler2 = Stuff.ToEulerianAngle(ikSettings.TargetOrientation);
                                //var eulerErr = euler2 - euler1;

                                if (ikSettings.IsRelative)
                                {
                                    if (ikSettings.IsComRelative)
                                    {
                                        jac = Stuff.ToMathNetMatrix(IkCalculator.CalcJacobianOrientationExternalFrame(Joints, Stuff.ToVector3(target), new Vector3(com.X, com.Y, com.Z)));

                                        de[0, 0] = (target.X - EndEffector.AbsolutePosition.X + com.X) * ikDelta;
                                        de[1, 0] = (target.Y - EndEffector.AbsolutePosition.Y + com.Y) * ikDelta;
                                        de[2, 0] = (target.Z - EndEffector.AbsolutePosition.Z + com.Z) * ikDelta;
                                        LastPositionError = target - EndEffector.AbsolutePosition + Stuff.ToVector3(com);

                                    }
                                    else
                                    {
                                        jac = Stuff.ToMathNetMatrix(IkCalculator.CalcJacobianOrientationExternalFrame(Joints, Stuff.ToVector3(target), ikSettings.PivotBone.AbsolutePosition));

                                        de[0, 0] = (target.X - EndEffector.AbsolutePosition.X + ikSettings.PivotBone.AbsolutePosition.X) * ikDelta;
                                        de[1, 0] = (target.Y - EndEffector.AbsolutePosition.Y + ikSettings.PivotBone.AbsolutePosition.Y) * ikDelta;
                                        de[2, 0] = (target.Z - EndEffector.AbsolutePosition.Z + ikSettings.PivotBone.AbsolutePosition.Z) * ikDelta;

                                    }

                                    /*
                                        jac = Stuff.ToMathNetMatrix(IkCalculator.CalcJacobianOrientationExternalFrame(Joints, Stuff.ToVector3(target), ikSettings.PivotBone.AbsolutePosition));

                                        de[0, 0] = (target.X - EndEffector.AbsolutePosition.X +
                                                    ikSettings.PivotBone.AbsolutePosition.X)*ikDelta;
                                        de[1, 0] = (target.Y - EndEffector.AbsolutePosition.Y +
                                                    ikSettings.PivotBone.AbsolutePosition.Y)*ikDelta;
                                        de[2, 0] = (target.Z - EndEffector.AbsolutePosition.Z +
                                                    ikSettings.PivotBone.AbsolutePosition.Z)*ikDelta;*/
                                }
                                else
                                {

                                    de[0, 0] = (target.X - EndEffector.AbsolutePosition.X) * ikDelta;
                                    de[1, 0] = (target.Y - EndEffector.AbsolutePosition.Y) * ikDelta;
                                    de[2, 0] = (target.Z - EndEffector.AbsolutePosition.Z) * ikDelta;
                                }

                                //de[3, 0] = (float)(eulerErr.X) * ikDelta2;
                                //de[4, 0] = (float)(eulerErr.Y) * ikDelta2;
                                //de[5, 0] = (float)(eulerErr.Z) * ikDelta2;

                                ///

                                var euler1 = Stuff.ToEulerianAngle(EndEffector.Parent.Orientation);
                                var erad = new Vector3d(MathHelper.DegreesToRadians(ikSettings.Euler.X), MathHelper.DegreesToRadians(ikSettings.Euler.Y), MathHelper.DegreesToRadians(ikSettings.Euler.Z));
                                //var euler2 = ikSettings.Euler;
                                var eulerErr = erad - euler1;
                                LastEulerError = new Vector3((float)eulerErr.X, (float)eulerErr.Y, (float)eulerErr.Z);


                                de[3, 0] = (float)(eulerErr.X) * ikDelta2;
                                de[4, 0] = (float)(eulerErr.Y) * ikDelta2;
                                de[5, 0] = (float)(eulerErr.Z) * ikDelta2;

                                /*de[3, 0] = (q.X);
                                    de[4, 0] = (q.Y);
                                    de[5, 0] = (q.Z);*/

                                //deltaE=B(g-e)
                            }
                            break;
                        case IkTypeEnum.Position3D:
                            {
                                jac = Stuff.ToMathNetMatrix(IkCalculator.CalcJacobian(Joints, Stuff.ToVector3(target)));
                           
                                //transpose jac

                                de = new DenseMatrix(3, 1);
                                if (ikSettings.IsRelative)
                                {
                                    if (ikSettings.IsComRelative)
                                    {
                                        jac = Stuff.ToMathNetMatrix(IkCalculator.CalcJacobianExternalFrame(Joints, Stuff.ToVector3(target), new Vector3(com.X, com.Y, com.Z)));

                                        de[0, 0] = (target.X - EndEffector.AbsolutePosition.X + com.X) * ikDelta;
                                        de[1, 0] = (target.Y - EndEffector.AbsolutePosition.Y + com.Y) * ikDelta;
                                        de[2, 0] = (target.Z - EndEffector.AbsolutePosition.Z + com.Z) * ikDelta;

                                    }
                                    else
                                    {
                                        jac = Stuff.ToMathNetMatrix(IkCalculator.CalcJacobianExternalFrame(Joints, Stuff.ToVector3(target), ikSettings.PivotBone.AbsolutePosition));

                                        de[0, 0] = (target.X - EndEffector.AbsolutePosition.X + ikSettings.PivotBone.AbsolutePosition.X) * ikDelta;
                                        de[1, 0] = (target.Y - EndEffector.AbsolutePosition.Y + ikSettings.PivotBone.AbsolutePosition.Y) * ikDelta;
                                        de[2, 0] = (target.Z - EndEffector.AbsolutePosition.Z + ikSettings.PivotBone.AbsolutePosition.Z) * ikDelta;

                                    }

                                }
                                else
                                {
                                    de[0, 0] = (target.X - EndEffector.AbsolutePosition.X) * ikDelta;
                                    de[1, 0] = (target.Y - EndEffector.AbsolutePosition.Y) * ikDelta;
                                    de[2, 0] = (target.Z - EndEffector.AbsolutePosition.Z) * ikDelta;
                                }


                                
                            }
                            break;

                        //case IkTypeEnum.Position2Target:
                        //    {
                        //        jac = CalcJacobian2PositionTarget(new IkBone[] { Stuff.Registers.EndEffector, Stuff.Registers.EndEffector2 }, Stuff.Registers.IkJoints, new[] { Stuff.ToVector3(target), ikSettings.TargetPosition2 });
                        //        var t2 = ikSettings.TargetPosition2;
                        //        de = new float[6, 1];

                        //        de[0, 0] = (target.X - Stuff.Registers.EndEffector.AbsolutePosition.X) * ikDelta;
                        //        de[1, 0] = (target.Y - Stuff.Registers.EndEffector.AbsolutePosition.Y) * ikDelta;
                        //        de[2, 0] = (target.Z - Stuff.Registers.EndEffector.AbsolutePosition.Z) * ikDelta;

                        //        de[3, 0] = (t2.X - Stuff.Registers.EndEffector2.AbsolutePosition.X) * ikDelta;
                        //        de[4, 0] = (t2.Y - Stuff.Registers.EndEffector2.AbsolutePosition.Y) * ikDelta;
                        //        de[5, 0] = (t2.Z - Stuff.Registers.EndEffector2.AbsolutePosition.Z) * ikDelta;

                        //        /*de[3, 0] = (q.X - Stuff.Registers.EndEffector.Parent.Orientation.X);
                        //        de[4, 0] = (q.Y - Stuff.Registers.EndEffector.Parent.Orientation.Y);
                        //        de[5, 0] = (q.Z - Stuff.Registers.EndEffector.Parent.Orientation.Z);*/
                        //        //deltaE=B(g-e)
                        //    }
                        //    break;

                        case IkTypeEnum.Orientation3D:
                            {
                                jac = Stuff.ToMathNetMatrix(IkCalculator.CalcJacobianOrientationOnly(Joints));

                                //transpose jac

                                de = new DenseMatrix(3, 1);
                                var q = ikSettings.TargetOrientation;
                                q.Normalize();
                                var q2 = EndEffector.Parent.Orientation.Normalized();
                                var err = q * q2;


                                var euler1 = Stuff.ToEulerianAngle(EndEffector.Parent.Orientation);
                                var erad = new Vector3d(MathHelper.DegreesToRadians(ikSettings.Euler.X), MathHelper.DegreesToRadians(ikSettings.Euler.Y), MathHelper.DegreesToRadians(ikSettings.Euler.Z));
                                //var euler2 = ikSettings.Euler;
                                var eulerErr = erad - euler1;


                                de[0, 0] = (float)(eulerErr.X) * ikDelta2;
                                de[1, 0] = (float)(eulerErr.Y) * ikDelta2;
                                de[2, 0] = (float)(eulerErr.Z) * ikDelta2;
                                //deltaE=B(g-e)
                            }
                            break;
                    }

                    for (int i = 0; i < de.RowCount; i++)
                    {
                        de[i, 0] *= DeMask[i];
                    }
                    var j = jac;

                    //f.SetMatrix(deltaE, "de");
                    string ret = "";
                    Matrix<double> jin = null;

                    if (ikSettings.Method == IkMethodEnum.JacobTranspose)
                    {
                        jin = j.Transpose();                        
                    }
                    if (ikSettings.Method == IkMethodEnum.JacobPinv)
                    {
                        jin = j.PseudoInverse();
                    }
                    
                    dlo = jin * de;
                    
                    var notPinned = Joints.Where(z => z.Pinned == false).ToArray();
                    for (int i = 0; i < notPinned.Count(); i++)
                    {                        
                        var vv = (float)(dlo[i, 0]);
                        if (!float.IsNaN(vv) && !float.IsInfinity(vv))
                        {
                            notPinned[i].Rotate += vv;
                            notPinned[i].FixLimit();
                        }                        
                    }
                }
                return Stuff.FromMathNetMatrix(de);
            }
            #endregion
            return null;
        }
    }

    public class SceneXmlState
    {
        public string Key;
        public string[] State = null;
        public List<StatePart> Parts = new List<StatePart>();
    }

    public class StatePart
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float[] Values;
        public object Tag;
    }

    public class Line
    {
        public PointF Start;
        public PointF End;
    }
    public enum EditModeEnum
    {
        NotEdited, Edited
    }
    public class CloneItem
    {
        public object Original;
        public object Clone;
    }

}
