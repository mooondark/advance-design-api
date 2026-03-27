using AD.API.Client;

namespace Test.API.Client
{
  internal partial class Program
  {
    /// <summary>
    /// Custom properties verification sample:
    /// - Creates a portal frame (2 columns + 1 beam, C25/30 / R20*30) and a planar slab element.
    /// - Each element is created with explicit custom properties:
    ///     LinearElement 1 (column at x=0):  SectionOrientationAngle=20deg, AutoMesh=false
    ///     LinearElement 2 (column at x=6):  SectionOrientationAngle=45deg, AutoMesh=false
    ///     LinearElement 3 (beam at z=4):    SectionOrientationAngle=-90deg, ElementActive=false
    ///     PlanarElement  1 (slab at z=0):   ThicknessIn1stVertex=0.25m, Eccentricity=0.05m
    /// - Saves and closes the project, then reopens it.
    /// - Retrieves each element individually by its creation EID via GetElementsObject.
    /// - Verifies that the custom properties survive the close/reopen cycle.
    /// </summary>
    static void Sample_CustomPropertiesVerificationOnReopen(AD.API.Client.AD_Client client)
    {
      LogVerboseLevel logVerbosityLevel = LogVerboseLevel.ErrorsAndWarnings;

      Environments env = new Environments()
      {
        Localization = Localization_Code.LOCALIZATION_FRANCE,
        LogVerbosity  = logVerbosityLevel
      };

      string projectPath = GetEnvironmentVariable("AD_MODELS_PATH")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "Graitec", "Advance Design", "2027", "Projects")
           + Path.DirectorySeparatorChar;

      string savedProjectFile = projectPath + "CustomPropsVerificationOrReopen_" + Guid.NewGuid().ToString() + ".fto";

      // ── Create project ────────────────────────────────────────────────────────
      var respNewProj = client.NewProject(savedProjectFile, env);
      PrintResultDetails(respNewProj.Details, logVerbosityLevel, "NewProject");
      if (!respNewProj.Details.Success || respNewProj.Details.HasErrors)
      {
        Console.WriteLine("ERROR: NewProject failed. Aborting.");
        return;
      }

      // ── Create material C25/30 ────────────────────────────────────────────────
      var respMaterial = client.CreateMaterial(new Material { Name = "C25/30" });
      PrintResultDetails(respMaterial.Details, logVerbosityLevel, "CreateMaterial C25/30");
      if (!respMaterial.Details.Success || respMaterial.Details.HasErrors)
      {
        Console.WriteLine("ERROR: CreateMaterial C25/30 failed. Aborting.");
        client.CloseProject();
        return;
      }
      EID idMaterial = respMaterial.Data;
      Console.WriteLine("  Material OID: " + idMaterial.Value);

      // ── Create section R20*30 ─────────────────────────────────────────────────
      var respSection = client.CreateSection("R20*30");
      PrintResultDetails(respSection.Details, logVerbosityLevel, "CreateSection R20*30");
      if (!respSection.Details.Success || respSection.Details.HasErrors)
      {
        Console.WriteLine("ERROR: CreateSection R20*30 failed. Aborting.");
        client.CloseProject();
        return;
      }
      EID idSection = respSection.Data;
      Console.WriteLine("  Section OID: " + idSection.Value);

      // ── Custom property values to set and later verify ────────────────────────
      const double orientationAngle_col1 = 20.0  * Math.PI / 180.0;   // radians
      const double orientationAngle_col2 = 45.0  * Math.PI / 180.0;
      const double orientationAngle_beam = -90.0 * Math.PI / 180.0;
      const bool   autoMesh_col1         = false;
      const bool   autoMesh_col2         = false;
      const bool   elementActive_beam    = false;
      const double slabThickness         = 0.25;   // m
      const double slabEccentricity      = 0.05;   // m

      // ── Element 1: column at x=0, (0,0,0)→(0,0,4) ────────────────────────────
      EID idCol1;
      {
        var el = new ElementLinear
        {
          Section                 = idSection,
          Material                = idMaterial,
          GeomPtStart             = new Pt3D { X = 0.0, Y = 0.0, Z = 0.0 },
          GeomPtEnd               = new Pt3D { X = 0.0, Y = 0.0, Z = 4.0 },
          SectionOrientationAngle = orientationAngle_col1,
          AutoMesh                = autoMesh_col1
        };
        var resp = client.CreateElement(el);
        PrintResultDetails(resp.Details, logVerbosityLevel,
          $"CreateElement col1 (OrientAngle={orientationAngle_col1:F4} rad, AutoMesh={autoMesh_col1})");
        idCol1 = resp.Data;
        Console.WriteLine("  Col1 OID: " + idCol1.Value);
      }

      // ── Element 2: column at x=6, (6,0,0)→(6,0,4) ────────────────────────────
      EID idCol2;
      {
        var el = new ElementLinear
        {
          Section                 = idSection,
          Material                = idMaterial,
          GeomPtStart             = new Pt3D { X = 6.0, Y = 0.0, Z = 0.0 },
          GeomPtEnd               = new Pt3D { X = 6.0, Y = 0.0, Z = 4.0 },
          SectionOrientationAngle = orientationAngle_col2,
          AutoMesh                = autoMesh_col2
        };
        var resp = client.CreateElement(el);
        PrintResultDetails(resp.Details, logVerbosityLevel,
          $"CreateElement col2 (OrientAngle={orientationAngle_col2:F4} rad, AutoMesh={autoMesh_col2})");
        idCol2 = resp.Data;
        Console.WriteLine("  Col2 OID: " + idCol2.Value);
      }

      // ── Element 3: beam, (0,0,4)→(6,0,4) ────────────────────────────────────
      EID idBeam;
      {
        var el = new ElementLinear
        {
          Section                 = idSection,
          Material                = idMaterial,
          GeomPtStart             = new Pt3D { X = 0.0, Y = 0.0, Z = 4.0 },
          GeomPtEnd               = new Pt3D { X = 6.0, Y = 0.0, Z = 4.0 },
          SectionOrientationAngle = orientationAngle_beam,
          ElementActive           = elementActive_beam,
         
          LinearElementType = LinearElementFEMType_API.ELinearElementFEMTypeGeneral,
          GeneralBeamType = GeneralBeamType_API.Sbeam,          

          DetailedMeshProperties = new LinearElementMeshStyle
          {
            NumberOfElements = 5,
            SizeOfElements = 1.0,
            SpacingOption = 0,
            SpacingFactor = 1.0
          },
          ExtendIntoWall = true,

          SectionExcentration = new SectionOffsetStyle
          {
            Option = SectionOffsetOption_API.Center_alignment,
            DeltaY1 = 1.0,
            DeltaZ1 = 0.5,
            DeltaY2 = 0.2,
            DeltaZ2 = 0.1,
            ConsideredInFEM = false
          },
          InertiaProperties = new LinearElementInnertia
          {
            LinearConcreteInertiaType = 0,
            LinearCoeffCrackedInertia = 1.0,
            LinearCoeffCrackedInertiaIz = 1.2,
            LinearCoeffCrackedInertiaTorsion = 0.5,
            LinearCoeffCrackedInertiaAxialStiff = 1.3,
            ProductOfInertiaIyz0 = false
          },
          InitialConstraintProperties = new InitialConstraint
          {
            InitialConstraintLoadCase = new EID { Value = 0 },//could be caseID
            InitialConstraintSxx = 0.0,
            ConstraintSxy = 0.0,
            ConstraintSxz = 0.0
          },
          RelaxationTotale = new ConnectionTotale
          {
            StartBoundaryConnection = new ConnectionStyle
            {
              RelaxationTx = false,
              RelaxationTy = false,
              RelaxationTz = false,
              RelaxationRx = false,
              RelaxationRy = false,
              RelaxationRz = false
            },
            EndBoundaryConnection = new ConnectionStyle
            {
              RelaxationTx = false,
              RelaxationTy = false,
              RelaxationTz = false,
              RelaxationRx = false,
              RelaxationRy = false,
              RelaxationRz = false
            }
          },
          RelaxationElastique = new ConnectionElastique
          {
            StartConnectionElastique = new ConnectionElastiqueObject
            {
              ElastiqueRelaxationTxValues = new List<FunctionPoint>
            {
              new FunctionPoint { ValueX = 0.0, ValueFX = 1000.0 },
              new FunctionPoint { ValueX = 0.01, ValueFX = 500.0 },
              new FunctionPoint { ValueX = 0.1, ValueFX = 100.0 }
            },
              ElastiqueRelaxationTyValues = null,
              ElastiqueRelaxationTzValues = new List<FunctionPoint>
            {
              new FunctionPoint { ValueX = 0.0, ValueFX = 2000.0 },
              new FunctionPoint { ValueX = 0.01, ValueFX = 1000.0 },
              new FunctionPoint { ValueX = 0.1, ValueFX = 200.0 }
            },
              ElastiqueRelaxationRxValues = null,
              ElastiqueRelaxationRyValues = new List<FunctionPoint>
            {
              new FunctionPoint { ValueX = 0.0, ValueFX = 500.0 },
              new FunctionPoint { ValueX = 0.01, ValueFX = 250.0 },
              new FunctionPoint { ValueX = 0.1, ValueFX = 50.0 }
            },
              ElastiqueRelaxationRzValues = null
            },
            EndConnectionElastique = new ConnectionElastiqueObject
            {
              ElastiqueRelaxationTxValues = null,
              ElastiqueRelaxationTyValues = null,
              ElastiqueRelaxationTzValues = new List<FunctionPoint>
            {
              new FunctionPoint { ValueX = 0.0, ValueFX = 2000.0 },
              new FunctionPoint { ValueX = 0.01, ValueFX = 1000.0 },
              new FunctionPoint { ValueX = 0.1, ValueFX = 200.0 }
            },
              ElastiqueRelaxationRxValues = new List<FunctionPoint>
            {
              new FunctionPoint { ValueX = 0.0, ValueFX = 500.0 },
              new FunctionPoint { ValueX = 0.01, ValueFX = 250.0 },
              new FunctionPoint { ValueX = 0.1, ValueFX = 50.0 }
            },
              ElastiqueRelaxationRyValues = null,
              ElastiqueRelaxationRzValues = null
            }
          },
          HaunchStart = new HaunchProperties
          {
            HaunchPosition = HaunchPosition_API.No_haunch,
            LengthType = HaunchLengthSizeType_API.Ratio,
            LengthRatio = 0.2,
            LengthValue = 0.5,
            HaunchSectionType = HaunchSectionType_API.Identical,
            HaunchSection = idSection,
            HeightType = HaunchLengthSizeType_API.Ratio,
            HeightRatio = 1.0,
            HeightValue = 0.2
          },
          HaunchEnd = new HaunchProperties
          {
            HaunchPosition = HaunchPosition_API.No_haunch,
            LengthType = HaunchLengthSizeType_API.Ratio,
            LengthRatio = 0.3,
            LengthValue = 0.6,
            HaunchSectionType = HaunchSectionType_API.Identical,
            HaunchSection = idSection,
            HeightType = HaunchLengthSizeType_API.Ratio,
            HeightRatio = 1.1,
            HeightValue = 0.3
          },
          Clipping = new ClippingProperties
          {
            ClippingState = false,
            StartClipping = new ClippingExtremity
            {
              PlanXY = ClippingCalculationType_API.Clipping_auto,
              ValueXY = 0.3,
              PlanXZ = ClippingCalculationType_API.Clipping_auto,
              ValueXZ = 0.10
            },
            EndClipping = new ClippingExtremity
            {
              PlanXY = ClippingCalculationType_API.Clipping_auto,
              ValueXY = 0.35,
              PlanXZ = ClippingCalculationType_API.Clipping_auto,
              ValueXZ = 0.25
            }
          },
          LoadAreaLoadTransferProperties = new LinearLoadAreaLoadTransfer
          {
            IsWindWallSupport = true,
            IsScaffoldingSupport = true,
            ClimaticLE2D = true,
            LoadSpanFront = 3.5,
            LoadSpanBehind = 1.5,
            ContinuityCoeff = 1.2,
            OpeningType = LoadArea_Opening_type.CG_WINDWALL_OPENING_TYPE_CLOSED_NO_OPENINGS,
            OpeningsValue = 0.1,
            SnowGuards2D = false
          }
        };

        var resp = client.CreateElement(el);
        PrintResultDetails(resp.Details, logVerbosityLevel,
          $"CreateElement beam (OrientAngle={orientationAngle_beam:F4} rad, ElementActive={elementActive_beam})");
        idBeam = resp.Data;
        Console.WriteLine("  Beam OID: " + idBeam.Value);
      }

      // ── Element 4: slab, quad (0,0,0)→(6,0,0)→(6,6,0)→(0,6,0) ─────────────
      EID idSlab;
      {
        var el = new ElementPlanar
        {
          Material             = idMaterial,
          ThicknessIn1stVertex = slabThickness,
          Eccentricity         = slabEccentricity,
          GeomPtsList          = new List<Pt3D>
          {
            new Pt3D { X = 0.0, Y = 0.0, Z = 0.0 },
            new Pt3D { X = 6.0, Y = 0.0, Z = 0.0 },
            new Pt3D { X = 6.0, Y = 6.0, Z = 0.0 },
            new Pt3D { X = 0.0, Y = 6.0, Z = 0.0 }
          }
        };
        var resp = client.CreateElement(el);
        PrintResultDetails(resp.Details, logVerbosityLevel,
          $"CreateElement slab (Thickness={slabThickness}m, Eccentricity={slabEccentricity}m)");
        idSlab = resp.Data;
        Console.WriteLine("  Slab OID: " + idSlab.Value);
      }

      // 2 rigid supports at column bases with concrete material
      var _support1resp = client.CreateElement(new ElementRigidPunctualSupport
      {
        GeomPt = new Pt3D { X = 0.0, Y = 0.0, Z = 0.0 },
        Material = idMaterial
      });
      PrintResultDetails(_support1resp.Details, logVerbosityLevel, "create 1st support");

     var _support2resp = client.CreateElement(new ElementRigidPunctualSupport
      {
        GeomPt = new Pt3D { X = 6.0, Y = 0.0, Z = 0.0 },
        Material = idMaterial
      });
      PrintResultDetails(_support2resp.Details, logVerbosityLevel, "create 2nd support");


      // ── Close project (save) ─────────────────────────────────────────────────
      var respClose = client.CloseProject();
      PrintResultDetails(respClose.Details, logVerbosityLevel, "CloseProject (save)");
      Console.WriteLine("  Project saved to: " + savedProjectFile);

      // ── Reopen project ────────────────────────────────────────────────────────
      var respOpen = client.OpenProject(savedProjectFile, env);
      PrintResultDetails(respOpen.Details, logVerbosityLevel, "OpenProject (reopen)");
      if (!respOpen.Details.Success || respOpen.Details.HasErrors)
      {
        Console.WriteLine("ERROR: OpenProject failed. Aborting verification.");
        return;
      }

      // ── Verify custom properties ─────────────────────────────────────────────
      Console.WriteLine();
      Console.WriteLine("=== Custom properties verification ===");

      const double angleTolerance  = 1e-6;
      const double lengthTolerance = 1e-6;
      bool allPassed = true;

      bool CheckProp<T>(string elementLabel, string propName, T expected, T actual, bool pass)
      {
        if (pass)
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"  [PASS] {elementLabel} — {propName}: expected={expected}, actual={actual}");
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"  [FAIL] {elementLabel} — {propName}: expected={expected}, actual={actual}");
          allPassed = false;
        }
        Console.ForegroundColor = ConsoleColor.White;
        return pass;
      }

      // ── Col1: retrieve by EID, verify SectionOrientationAngle + AutoMesh ─────
      {
        var resp = client.GetElementsObject(new List<long> { idCol1.Value });
        PrintResultDetails(resp.Details, logVerbosityLevel, $"GetElementsObject col1 (EID={idCol1.Value})");
        if (resp.Data?.FirstOrDefault() is ElementLinear lin)
        {
          CheckProp($"col1(EID={idCol1.Value})", "SectionOrientationAngle", orientationAngle_col1, lin.SectionOrientationAngle ?? 0.0, Math.Abs((lin.SectionOrientationAngle ?? 0.0) - orientationAngle_col1) <= angleTolerance);
          CheckProp($"col1(EID={idCol1.Value})", "AutoMesh",                autoMesh_col1,         lin.AutoMesh ?? true,               lin.AutoMesh == autoMesh_col1);
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"  [FAIL] col1(EID={idCol1.Value}): element not found or unexpected type.");
          Console.ForegroundColor = ConsoleColor.White;
          allPassed = false;
        }
      }

      // ── Col2: retrieve by EID, verify SectionOrientationAngle + AutoMesh ─────
      {
        var resp = client.GetElementsObject(new List<long> { idCol2.Value });
        PrintResultDetails(resp.Details, logVerbosityLevel, $"GetElementsObject col2 (EID={idCol2.Value})");
        if (resp.Data?.FirstOrDefault() is ElementLinear lin)
        {
          CheckProp($"col2(EID={idCol2.Value})", "SectionOrientationAngle", orientationAngle_col2, lin.SectionOrientationAngle ?? 0.0, Math.Abs((lin.SectionOrientationAngle ?? 0.0) - orientationAngle_col2) <= angleTolerance);
          CheckProp($"col2(EID={idCol2.Value})", "AutoMesh",                autoMesh_col2,         lin.AutoMesh ?? true,               lin.AutoMesh == autoMesh_col2);
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"  [FAIL] col2(EID={idCol2.Value}): element not found or unexpected type.");
          Console.ForegroundColor = ConsoleColor.White;
          allPassed = false;
        }
      }

      // ── Beam: retrieve by EID, verify SectionOrientationAngle + ElementActive ─
      {
        var resp = client.GetElementsObject(new List<long> { idBeam.Value });
        PrintResultDetails(resp.Details, logVerbosityLevel, $"GetElementsObject beam (EID={idBeam.Value})");
        if (resp.Data?.FirstOrDefault() is ElementLinear lin)
        {
          CheckProp($"beam(EID={idBeam.Value})", "SectionOrientationAngle", orientationAngle_beam, lin.SectionOrientationAngle ?? 0.0, Math.Abs((lin.SectionOrientationAngle ?? 0.0) - orientationAngle_beam) <= angleTolerance);
          CheckProp($"beam(EID={idBeam.Value})", "ElementActive",           elementActive_beam,    lin.ElementActive ?? true,          lin.ElementActive == elementActive_beam);
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"  [FAIL] beam(EID={idBeam.Value}): element not found or unexpected type.");
          Console.ForegroundColor = ConsoleColor.White;
          allPassed = false;
        }
      }

      // ── Slab: retrieve by EID, verify ThicknessIn1stVertex + Eccentricity ────
      {
        var resp = client.GetElementsObject(new List<long> { idSlab.Value });
        PrintResultDetails(resp.Details, logVerbosityLevel, $"GetElementsObject slab (EID={idSlab.Value})");
        if (resp.Data?.FirstOrDefault() is ElementPlanar pl)
        {
          CheckProp($"slab(EID={idSlab.Value})", "ThicknessIn1stVertex", slabThickness,    pl.ThicknessIn1stVertex, Math.Abs(pl.ThicknessIn1stVertex - slabThickness)  <= lengthTolerance);
          CheckProp($"slab(EID={idSlab.Value})", "Eccentricity",         slabEccentricity, pl.Eccentricity,         Math.Abs(pl.Eccentricity - slabEccentricity)        <= lengthTolerance);
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"  [FAIL] slab(EID={idSlab.Value}): element not found or unexpected type.");
          Console.ForegroundColor = ConsoleColor.White;
          allPassed = false;
        }
      }

      Console.WriteLine();
      if (allPassed)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("VERIFICATION PASSED: all custom properties were preserved after close/reopen.");
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("VERIFICATION FAILED: one or more custom properties were not preserved. Check results above.");
      }
      Console.ForegroundColor = ConsoleColor.White;

      var respFinalClose = client.CloseProject();
      PrintResultDetails(respFinalClose.Details, logVerbosityLevel, "CloseProject");
    }
  }
}
