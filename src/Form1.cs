using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using VIZCore3D.NET;
using VIZCore3D.NET.Data;
using VIZCore3D.NET.Interop;

namespace VIZToHMF
{
    public partial class HMFForm : Form
    {
        private VIZCore3D.NET.VIZCore3DControl vizcore3d;

        private List<Structure> structures;
        private List<PropTable> propTables;
        private List<MeshTable> meshTables;
        private uint prop_table_count;
        private uint struct_count;
        private uint mesh_table_count;

        string inputPath;
        string outputPath;

        public HMFForm()
        {
            InitializeComponent();

            // Initialize VIZCore3D.NET
            VIZCore3D.NET.ModuleInitializer.Run();

            // Construction
            vizcore3d = new VIZCore3DControl();
            vizcore3d.Dock = DockStyle.Fill;
            panelView.Controls.Add(vizcore3d);

            // 필수 Event
            vizcore3d.OnInitializedVIZCore3D += VIZCore3D_OnInitializedVIZCore3D;
        }

        public HMFForm(string[] args)
        {     
            InitializeComponent();

            // Initialize VIZCore3D.NET
            VIZCore3D.NET.ModuleInitializer.Run();

            if (args != null && args.Length == 2)
            {
                inputPath = args[0];    //입력 파라미터에서 변환할 원본 파일의 경로를 가져옵니다.
                outputPath = args[1];   //입력 파라미터에서 변환 후 출력할 파일의 경로를 가져옵니다.
            }
            else
            {
                Application.Exit();     //입력 파라미터가 없을 경우 응용프로그램을 종료합니다.
                return;
            }

            // Construction
            vizcore3d = new VIZCore3DControl();
            vizcore3d.Dock = DockStyle.Fill;
            panelView.Controls.Add(vizcore3d);
            // 필수 Event
            vizcore3d.OnInitializedVIZCore3D += VIZCore3D_OnInitializedVIZCore3D;
        }


        private void VIZCore3D_OnInitializedVIZCore3D(object sender, EventArgs e)
        {
            // Init. VIZCore3D.NET
            InitializeVIZCore3D();
            
            // 중립포멧으로 출력하는 API를 호출합니다.
            VIZToHMF();
        }

        // ================================================
        // Function - VIZCore3D.NET : Initialize
        // ================================================
        #region Function - VIZCore3D.NET : Initialize
        private void InitializeVIZCore3D()
        {
            // ================================================================
            // 모델 열기 시, 3D 화면 Rendering 차단
            // ================================================================
            vizcore3d.BeginUpdate();


            // ================================================================
            // 설정 - 기본
            // ================================================================
            #region 설정 - 기본
            // 모델 자동 언로드 (파일 노드 언체크 시, 언로드)
            vizcore3d.Model.UncheckToUnload = true;

            // 모델 열기 시, Edge 정보 로드 활성화
            vizcore3d.Model.LoadEdgeData = true;

            // 모델 열기 시, Edge 정보 생성 활성화
            vizcore3d.Model.GenerateEdgeData = true;

            // 모델 조회 시, 하드웨어 가속
            vizcore3d.View.EnableHardwareAcceleration = true;

            // Undo/Redo (비)활성화
            vizcore3d.Model.EnableUndoRedo = false;

            // 모델 열기 시, 스트럭처 병합 설정
            vizcore3d.Model.OpenMergeStructureMode = VIZCore3D.NET.Data.MergeStructureModes.NONE;

            // 모델 저장 시, 스트럭처 병합 설정
            vizcore3d.Model.SaveMergeStructureMode = VIZCore3D.NET.Data.MergeStructureModes.NONE;

            // 실린더 원형 품질 개수 : Nomal(12~36), Small(6~36)
            vizcore3d.Model.ReadNormalCylinderSide = 12;
            vizcore3d.Model.ReadSmallCylinderSide = 6;

            // 보이는 모델만 저장

            // VIZXML to VIZ 옵션
            vizcore3d.Model.VIZXMLtoVIZOption = VIZCore3D.NET.Data.ExportVIZXMLToVIZOptions.LOAD_UNLOADED_NODE;

            // 선택 가능 개체 : 전체, 불투명한 개체
            vizcore3d.View.SelectionObject3DType = VIZCore3D.NET.Data.SelectionObject3DTypes.ALL;

            // 개체 선택 유형 : 색상, 경계로 선택 (개체), 경계로 선택 (전체)
            vizcore3d.View.SelectionMode = VIZCore3D.NET.Data.Object3DSelectionOptions.HIGHLIGHT_COLOR;

            // 개체 선택 색상
            vizcore3d.View.SelectionColor = Color.Red;

            // 모델 조회 시, Pre-Select 설정
            vizcore3d.View.PreSelect.Enable = false;

            // 모델 조회 시, Pre-Select 색상 설정
            vizcore3d.View.PreSelect.HighlightColor = Color.Lime;

            // 모델 조회 시, Pre-Select Label
            vizcore3d.View.PreSelect.Label = VIZCore3D.NET.Data.PreSelectStyle.LabelKind.HIERACHY_BOTTOM_UP;

            // 모델 조회 시, Pre-Select Font
            vizcore3d.View.PreSelect.LabelFont = new Font("Arial", 10.0f);
            #endregion


            // ================================================================
            // 설정 - 보기
            // ================================================================
            #region 설정 - 보기
            // 자동 애니메이션 : 박스줌, 개체로 비행 등 기능에서 애니메이션 활성화/비활성화
            vizcore3d.View.EnableAnimation = true;

            // 자동화면맞춤
            vizcore3d.View.EnableAutoFit = false;

            // 연속회전모드
            vizcore3d.View.EnableInertiaRotate = false;

            // 확대/축소 비율 : 5.0f ~ 50.0f
            vizcore3d.View.ZoomRatio = 30.0f;

            // 회전각도
            vizcore3d.View.RotationAngle = 90.0f;

            // 회전 축
            vizcore3d.View.RotationAxis = VIZCore3D.NET.Data.Axis.X;
            #endregion


            // ================================================================
            // 설정 - 탐색
            // ================================================================
            #region 설정 - 탐색
            // Z축 고정
            vizcore3d.Walkthrough.LockZAxis = true;
            // 선속도 : m/s
            vizcore3d.Walkthrough.Speed = 2.0f;
            // 각속도
            vizcore3d.Walkthrough.AngularSpeed = 30.0f;
            // 높이
            vizcore3d.Walkthrough.AvatarHeight = 1800.0f;
            // 반지름
            vizcore3d.Walkthrough.AvatarCollisionRadius = 400.0f;
            // 숙임높이
            vizcore3d.Walkthrough.AvatarBowWalkHeight = 1300.0f;
            // 충돌
            vizcore3d.Walkthrough.UseAvatarCollision = false;
            // 중력
            vizcore3d.Walkthrough.UseAvatarGravity = false;
            // 숙임
            vizcore3d.Walkthrough.UseAvatarBowWalk = false;
            // 모델
            vizcore3d.Walkthrough.AvatarModel = (int)VIZCore3D.NET.Data.AvatarModels.MAN1;
            // 자동줌
            vizcore3d.Walkthrough.EnableAvatarAutoZoom = false;
            // 충돌상자보기
            vizcore3d.Walkthrough.ShowAvatarCollisionCylinder = false;
            #endregion


            // ================================================================
            // 설정 - 조작
            // ================================================================
            #region 설정 - 조작
            // 시야각
            vizcore3d.View.FOV = 60.0f;
            // 광원 세기
            vizcore3d.View.SpecularGamma = 30.0f;
            // 모서리 굵기
            vizcore3d.View.EdgeWidthRatio = 0.0f;
            // X-Ray 모델 조회 시, 개체 색상 - 선택색상, 모델색상
            vizcore3d.View.XRay.ColorType = VIZCore3D.NET.Data.XRayColorTypes.SELECTION_COLOR;
            // 배경유형
            //vizcore3d.View.BackgroundMode = VIZCore3D.NET.Data.BackgroundModes.COLOR_ONE;
            // 배경색1
            //vizcore3d.View.BackgroundColor1 = Color.Gray;
            // 배경색2
            //vizcore3d.View.BackgroundColor2 = Color.Gray; 
            #endregion


            // ================================================================
            // 설정 - 노트
            // ================================================================
            #region 설정 - 노트
            // 배경색
            vizcore3d.Review.Note.BackgroundColor = Color.Yellow;
            // 배경 투명
            vizcore3d.Review.Note.BackgroudTransparent = false;
            // 글자색
            vizcore3d.Review.Note.FontColor = Color.Black;
            // 글자 크기
            vizcore3d.Review.Note.FontSize = VIZCore3D.NET.Data.FontSizeKind.SIZE16;
            // 글자 굵게
            vizcore3d.Review.Note.FontBold = true;
            // 지시선(라인) 색상
            vizcore3d.Review.Note.LineColor = Color.White;
            // 지시선(라인) 두께
            vizcore3d.Review.Note.LineWidth = 2;
            // 지시선 중앙 연결
            vizcore3d.Review.Note.LinkArrowTailToText = VIZCore3D.NET.Manager.NoteManager.LinkArrowTailToTextKind.OUTLINE;
            // 화살표 색상
            vizcore3d.Review.Note.ArrowColor = Color.Red;
            // 화살표 두께
            vizcore3d.Review.Note.ArrowWidth = 10;
            // 텍스트상자 라인 색상
            vizcore3d.Review.Note.TextBoxLineColor = Color.Black;

            // 심볼 배경색
            vizcore3d.Review.Note.SymbolBackgroundColor = Color.Red;
            // 심볼 글자색
            vizcore3d.Review.Note.SymbolFontColor = Color.White;
            // 심볼 크기
            vizcore3d.Review.Note.SymbolSize = 10;
            // 심볼 글자 크기
            vizcore3d.Review.Note.SymbolFontSize = VIZCore3D.NET.Data.FontSizeKind.SIZE16;
            // 심볼 글자 굵게
            vizcore3d.Review.Note.SymbolFontBold = false;
            #endregion


            // ================================================================
            // 설정 - 측정
            // ================================================================
            #region 설정 - 측정
            // 반복 모드
            vizcore3d.Review.Measure.RepeatMode = false;

            // 기본 스타일
            VIZCore3D.NET.Data.MeasureStyle measureStyle = vizcore3d.Review.Measure.GetStyle();

            // Prefix 조회
            measureStyle.Prefix = true;
            // Frame(좌표계)로 표시
            measureStyle.Frame = true;
            // DX, DY, DZ 표시
            measureStyle.DX_DY_DZ = true;
            // 측정 단위 표시
            measureStyle.Unit = true;
            // 측정 단위 유형
            measureStyle.UnitKind = VIZCore3D.NET.Data.MeasureUnitKind.RUT_MILLIMETER;
            // 소수점 이하 자리수
            measureStyle.NumberOfDecimalPlaces = 2;
            // 연속거리 표시
            measureStyle.ContinuousDistance = true;

            // 배경 투명
            measureStyle.BackgroundTransparent = false;
            // 배경색
            measureStyle.BackgroundColor = Color.Blue;
            // 글자색
            measureStyle.FontColor = Color.White;
            // 글자크기
            measureStyle.FontSize = VIZCore3D.NET.Data.FontSizeKind.SIZE14;
            // 글자 두껍게
            measureStyle.FontBold = false;
            // 지시선 색
            measureStyle.LineColor = Color.White;
            // 지시선 두께
            measureStyle.LineWidth = 2;
            // 화살표 색
            measureStyle.ArrowColor = Color.Red;
            // 화살표 크기
            measureStyle.ArrowSize = 10;
            // 보조 지시선 표시
            measureStyle.AssistantLine = true;
            // 보조 지시선 표시 개수
            measureStyle.AssistantLineCount = -1;
            // 보조 지시선 라인 스타일
            measureStyle.AssistantLineStyle = VIZCore3D.NET.Data.MeasureStyle.AssistantLineType.DOTTEDLINE;
            // 선택 위치 표시
            measureStyle.PickPosition = true;
            // 거리 측정 텍스트 정렬
            measureStyle.AlignDistanceText = true;
            // 거리 측정 텍스트 위치
            measureStyle.AlignDistanceTextPosition = 2;
            // 거리 측정 텍스트 오프셋
            measureStyle.AlignDistanceTextMargine = 5;

            // 측정 스타일 설정
            vizcore3d.Review.Measure.SetStyle(measureStyle);
            #endregion


            // ================================================================
            // 설정 - 단면
            // ================================================================
            #region 설정 - 단면
            // 단면 좌표간격으로 이동
            vizcore3d.Section.MoveSectionByFrameGrid = true;
            // 단면 보기
            vizcore3d.Section.ShowSectionPlane = true;
            // 단면선 표시
            vizcore3d.Section.ShowSectionLine = true;
            // 단면 단일색 표시
            vizcore3d.Section.ShowSectionLineColor = false;
            // 단면 단일색
            vizcore3d.Section.SectionLineColor = Color.Red;
            #endregion


            // ================================================================
            // 설정 - 간섭검사
            // ================================================================
            // 다중간섭검사
            vizcore3d.Clash.EnableMultiThread = true;


            // ================================================================
            // 설정 - 프레임(SHIP GRID)
            // ================================================================
            #region 설정 - 프레임
            // 프레임 평면 설정
            vizcore3d.Frame.XYPlane = true;
            vizcore3d.Frame.YZPlane = true;
            vizcore3d.Frame.ZXPlane = true;
            vizcore3d.Frame.PlanLine = true;
            vizcore3d.Frame.SectionLine = true;
            vizcore3d.Frame.ElevationLine = true;

            // 좌표값 표기
            vizcore3d.Frame.ShowNumber = true;

            // 모델 앞에 표기
            vizcore3d.Frame.BringToFront = false;

            // Frame(좌표계, SHIP GRID) 색상
            vizcore3d.Frame.ForeColor = Color.Black;

            // 홀수번째 표시
            vizcore3d.Frame.ShowOddNumber = true;
            // 짝수번째 표시
            vizcore3d.Frame.ShowEvenNumber = true;
            // 단면상자에 자동 맞춤
            vizcore3d.Frame.AutoFitSectionBox = true;
            #endregion


            // ================================================================
            // 설정 - 툴바
            // ================================================================
            #region 설정 - 툴바
            vizcore3d.ToolbarMain.Visible = true;
            vizcore3d.ToolbarNote.Visible = false;
            vizcore3d.ToolbarMeasure.Visible = false;
            vizcore3d.ToolbarSection.Visible = false;
            vizcore3d.ToolbarClash.Visible = false;
            vizcore3d.ToolbarAnimation.Visible = false;
            vizcore3d.ToolbarSimulation.Visible = false;
            vizcore3d.ToolbarPrimitive.Visible = false;
            #endregion


            // ================================================================
            // 설정 - 상태바
            // ================================================================
            vizcore3d.Statusbar.Visible = false;


            // ================================================================
            // 모델 열기 시, 3D 화면 Rendering 재시작
            // ================================================================
            vizcore3d.EndUpdate();


            // ================================================================
            // 모델 BODY 활성화
            // ================================================================
            vizcore3d.Model.EnableBody = true;
        }
        #endregion


        private void VIZToHMF()
        {
            if (System.IO.File.Exists(inputPath) == false)
            {
                MessageBox.Show(string.Format("{0}파일이 존재하지 않습니다.", inputPath), "VIZToHMF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            //중립 포멧 구조체 초기화
            structures = new List<Structure>();
            propTables = new List<PropTable>();
            meshTables = new List<MeshTable>();

            //중립 포멧 구조체 인덱스를 관리할 변수 초기화
            prop_table_count = 0;
            struct_count     = 0;
            mesh_table_count = 0;

            if(vizcore3d.License.IsAuthentication() == false) Application.Exit();

            //경량 파일을 열기위한 API 호출
            vizcore3d.Model.OnModelProgressChangedEvent += Model_OnModelProgressChangedEvent; //모델 열기 프로그래스 정보 이벤트 처리
            bool bOpened = vizcore3d.Model.Open(inputPath); //경량화 모델 열기 API
            vizcore3d.Model.OnModelProgressChangedEvent -= Model_OnModelProgressChangedEvent; //모델 열기 프로그래스 정보 이벤트 처리

            if (bOpened == false)
            {
                MessageBox.Show(string.Format("{0}파일을 열수없습니다.", inputPath), "VIZToHMF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            try
            {
                List<VIZCore3D.NET.Data.Node> nodes = vizcore3d.Object3D.FromFilter(VIZCore3D.NET.Data.Object3dFilter.ROOT); //ROOT NODE를 가져온다.                                        

                //HMF CLASS로 데이터 재정의
                {

                    //ROOT NODE를 제외 후 하위 NODE만을 저장하기 위해 ROOT의 자식 노드를 가져온다.
                    List<VIZCore3D.NET.Data.Node> childnodes = nodes[0].GetChildObject3d(VIZCore3D.NET.Data.Object3DChildOption.CHILD_ONLY, true);
             
                    //자식 NODE의 개수 처리
                    int strCnt = childnodes.Count;
                  
                    //자식 NODE의 수만큼 반복하여 처리
                    for (int i = strCnt - 1; i >= 0; i--)
                    {
                        VIZCore3D.NET.Data.Node node = childnodes[i];

                        // 중립 포멧 Structure 생성 및 초기화
                        Structure structure = new Structure();

                        // 현재 NODE의 인덱스
                        structure.index = (uint)node.Index;

                        uint ptype = 0;

                        if (node.GetParent() != null)
                            ptype = (uint)node.GetParent().Kind;
                        uint type = (uint)node.Kind;
                    
                        structure.pIndex = node.ParentIndex;                 //현재 NODE의 부모 NODE 인덱스 값
                        structure.ptype = ptype;                             //현재 NODE의 부모 NODE의 TYPE 정보 => ASSEMBLY(0), PART(1), BODY(2), BODYSET(3)
                        structure.index = (uint)node.Index;                  //현재 NODE의 인덱스 값
                        structure.type = type;                               //현재 NODE의 TYPE 정보 => ASSEMBLY(0), PART(1), BODY(2), BODYSET(3)
                        structure.szName = node.NodeName;                    //현재 NODE의 이름
                        structure.bBox = node.GetBoundBox().ToArray();       //현재 NODE의 BOUNDBOX 정보 MIN, MAX 값
                                                                             
                        Matrix3D matrix = new Matrix3D();                    
                        matrix.Identity();                                   
                        structure.transform = matrix.ToArray();              //현재 NODE의 MATRIX 정보 이미 Mesh에 적용되어 있어 Identity Matrix정보로 대체

       
                        structure.propIDListCnt = (uint)node.GetUDA().Count; //현재 NODE의 사용자 정의 속성 개수

                        if (structure.propIDListCnt > 0)                     //NODE에 속성이 있을 경우에만 처리한다.
                        {
                            structure.propIDList = new List<uint>();         //사용자 정의 속성을 저장할 LIST 초기화
                            
                            Dictionary<string, string> uda = node.GetUDA();  //현재 NODE의 사용자 정의 속성을 가져온다. <Key, Value>
                            
                            structure.propIDList.Add(prop_table_count);      //전체 사용자 정의 속성 LIST의 Index값을 저장한다. => propTables의 Index 정보
                            PropTable pt_p = new PropTable();                //현재 NODE의 사용자 정의 속성을 담을 구조체를 초기화한다.
                            pt_p.index = prop_table_count;                   //현재 NODE 속성의 Index값을 부여한다.
                            pt_p.propCnt = structure.propIDListCnt;          //현재 NODE 속성의 전체 개수를 저장한다.

                            foreach (KeyValuePair<string, string> item in uda) //현재 NODE 속성의 개수만큼 반복한다.
                            {
                                pt_p.keyStr.Add(item.Key);                     //속성의 KEY값을 저장한다.
                                pt_p.valStr.Add(item.Value);                   //속성의 VALUE값을 저장한다.
                                pt_p.valType.Add(0);                           //속성은 기본적으로 STRING객체이므로 TYPE은 0을 저장한다.
                            }

                            pt_p.propCnt = (uint)pt_p.keyStr.Count;            //NODE에서 추출한 전체 속성개수와 실제 저장한 속성 개수가 상이할 수 있을 경우를 대비하여
                                                                               //실제 저장한 속성의 개수로 처리되도록 보정한다.

                            propTables.Add(pt_p);                              //현재 NODE의 속성을 담은 구조체를 전체 속성 TABLE에 저장한다.
                            prop_table_count++;                                //다음 NODE의 속성 Index를 부여하기 위해 전체 속성 Index관리 변수의 값을 +1한다. 
                        }

                           
                        if (structure.type == 2)                               //NODE의 TYPE이 형상이 있는 BODY일 경우에만 처리되도록 한다.
                        {
                            List<Facet> facets = node.GetBodyFacet();          //현재 NODE의 VERTEX, NORMAL, TRIANGLE INDEX, COLOR 정보를 FACET 구조체로 가져온다.

                            int facetCnt = facets.Count;                       //현재 NODE의 TRIANGLE SET 개수를 가져온다.
                            structure.meshIDListCnt = (uint)facetCnt;          //현재 NODE의 TRIANGLE SET 개수를 중립포멧구조체에 저장한다.
                            structure.meshIDList = new List<uint>();           //현재 NODE의 MESH정보를 담을 중립포멧 MESH구조체 LIST를 초기화한다.

                            for (int m = 0; m < facetCnt; m++)                 //TRIANGLE SET 개수만큼 반복하여 처리한다.
                            {
                                structure.meshIDList.Add(mesh_table_count);    //NODE MESHID관리 구조체에 현재 TRIANGLE SET의 Index를 추가한다.
                                MeshTable mt = new MeshTable();                //MESH 정보를 담을 구조체를 초기화한다.
                                mt.index = mesh_table_count;                   //MESH 정보의 index를 부여한다.
                                mesh_table_count++;                            //다음 MESH정보 index관리를 위해 전체 MESH Index관리 변수를 +1한다.

                                int vCnt = facets[m].Vertexes.Count;           //TRIANGLE SET의 Vertex 개수를 가져온다.
                                for (int v = 0; v < vCnt; v++)                 //Vertex개수 만큼 반복하며, Vertex / Normal 값을 List에 저장한다.
                                {
                                    mt.VArray.Add(facets[m].Vertexes[v]);
                                    mt.NArray.Add(facets[m].Normals[v]);
                                }
                            
                                int tCnt = facets[m].TirIndexes.Count;         //TRIANGLE SET의 Triangle Index 개수를 가져온다.
                                for (int v = 0; v < tCnt; v++)                 //Triangle Index 개수만큼 반복하며 Triangle Index값을 List에 저장한다.
                                    mt.TriArray.Add((ushort)facets[m].TirIndexes[v]);

                                mt.vnSize = (uint)(vCnt / 3);                  //Vertex/Normal의 개수를 저장한다. (x,y,z)가 1개의 Vertex이기 때문에 3으로 나눈 값을 저장한다.
                                mt.triSize = (uint)tCnt;                       //Triangle Index의 전체 개수를 저장한다.

                                // TRIANGLE SET의 RGBA 색상 정보를 저장한다. 
                                mt.color[0] = facets[m].Colors[0]; //RED
                                mt.color[1] = facets[m].Colors[1]; //BLUE
                                mt.color[2] = facets[m].Colors[2]; //GREEN
                                mt.color[3] = facets[m].Colors[3]; //ALPHA

                                meshTables.Add(mt);                            //현재 TRIANGLE SET MESH 정보를 전체 MESH 정보 관리 리스트에 저장한다.
                            }
                        }

                        struct_count++;  //전체 structures에서 자식 NODE를 찾기 휘한 Index를 정의하기 위해 +1한다. 

                        List<VIZCore3D.NET.Data.Node> children = node.GetChildObject3d(VIZCore3D.NET.Data.Object3DChildOption.CHILD_ONLY, true); //현재 NODE의 자식 NODE를 가져온다.
                        int childCnt = children.Count;                    //자식 NODE의 개수를 가져온다.
                        if (childCnt > 0)
                            structure.childNodeIDList = new List<uint>(); //자식 NODE가 있을 경우, 중립포멧 구조체 자식NODE의 Index를 관리할 LIST를 초기화한다.

                        for (int m = 0; m < childCnt; m++)                //자식 NODE의 개수만큼 반복한다.
                        {
                            VIZCore3D.NET.Data.Node cnode = children[m];  //자식 NODE리스트에서 1개의 NODE를 가져온다.
                            SetVIZStructureData(cnode, structure);        //가져온 NODE의 정보를 중립포멧 구조체에 저장하기 위해 API를 호출한다.
                        }

                        structure.childNodeIDListCnt = (uint)childCnt;    //현재 구조 정보에 자식 NODE의 개수를 저장한다.
                                                
                        structures.Add(structure);                        //전체 중립포멧 구조를 관리하는 LIST에 현재 정의한 구조 정보를 저장한다.
                    }   
                }

                //HMF 중립 포멧 파일 생성 및 쓰기
                using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write)) //파일을 생성하고 쓰기권한으로 FileStream을 생성합니다.
                {
                    using (BinaryWriter writer = new BinaryWriter(fs, Encoding.Unicode)) //FileStream을 Binary 형식으로 작성하기 위해 BinaryWriter를 초기화합니다.
                    {
                        // 버전 정보 작성
                        uint dwVersion = 0x00010003;
                        writer.Write(dwVersion);                //파일에 중립 포멧 파일 버전 정보를 저장합니다.

                        //구조체의 전체 개수 작성
                        int strCnt = structures.Count();
                        writer.Write(strCnt);                   //전체 구조 개수를 저장합니다.

                        Console.WriteLine($"WriteBinaryFile StrCnt: {strCnt}");

                        for (int i = strCnt - 1; i >= 0; i--)   //전체 구조 개수 만큼 반복하며 저장합니다.
                        {
                            Structure st = structures[i];       //전체 구조 정보 관리 LIST에서 현재 선택한 구조 정보만을 가져옵니다.

                            if (dwVersion >= 0x00010003)        
                            {
                                writer.Write(st.pIndex);        //부모 NODE의 Index를 저장합니다.
                                writer.Write(st.ptype);         //부모 NODE의 Type을 저장합니다. => ASSEMBLY(0), PART(1), BODY(2), BODYSET(3)
                            }

                            writer.Write(st.index);             //현재 NODE의 Index를 저장합니다.
                            writer.Write(st.type);              //현재 NODE의 Type을 저장합니다. => ASSEMBLY(0), PART(1), BODY(2), BODYSET(3)

                            // UTF-8로 변환
                            byte[] utf8Bytes = Encoding.UTF8.GetBytes(st.szName); //현재 NODE의 이름을 UTF8형식으로 인코딩합니다.
                            ulong size = (ulong)utf8Bytes.Length;                 //UTF8로 인코딩된 byte배열의 길이 값을 가져옵니다.

                            // UTF-8 문자열 크기 기록
                            writer.Write(size);

                            // UTF-8 문자열 데이터 기록
                            if (size > 0)
                            {
                                writer.Write(utf8Bytes); //UTF8 문자열을 저장합니다.
                            }

                            // 배열 크기만큼 BOUNDBOX [6] 데이터를 기록
                            foreach (float value in st.bBox)
                            {
                                writer.Write(value);
                            }

                            // 배열 크기만큼 MATRIX [16] 데이터를 기록
                            foreach (float value in st.transform)
                            {
                                writer.Write(value);
                            }

                            int propCont = st.propIDList.Count();
                            writer.Write(propCont);                              //현재 NODE의 사용자 정의 속성 Index List의 개수를 저장합니다.
                                                                                 //Index는 전체 사용자 정의 테이블 "propTables"의 index정보입니다. 

                            if (propCont > 0)
                            {
                                foreach (uint value in st.propIDList.ToArray())  // 배열 크기만큼 사용자 정의 속성 Index 값을 저장합니다.
                                {
                                    writer.Write(value);
                                }
                            }

                            int meshCont = st.meshIDList.Count();                
                            writer.Write(meshCont);                              //현재 NODE의 TRIANGLE SET MESH의 Index List의 개수를 저장합니다.
                                                                                 //Index는 전체 MESH 테이블 "meshTables"의 index정보입니다. 
                            if (meshCont > 0)
                            {
                                foreach (uint value in st.meshIDList.ToArray())  // 배열 크기만큼 TRIAGNLE SET MESH의 Index 값을 저장합니다.
                                {
                                    writer.Write(value);
                                }
                            }

                            int childCont = st.childNodeIDList.Count();
                            writer.Write(childCont);                                 // 현재 NODE의 자식 NODE Index List 개수를 저장합니다.
                            if (childCont > 0)
                            {
                                foreach (uint value in st.childNodeIDList.ToArray()) // 배열 크기만큼 자식 NODE의 Index 값을 저장합니다.
                                {
                                    writer.Write(value);
                                }
                            }
                        }

                        int propCnt = propTables.Count();
                        writer.Write(propCnt);                                       // 전체 NODE별 속성 관리 테이블의 개수를 저장합니다.

                        for (int i = 0; i < prop_table_count; i++)                   // 속성 관리 테이블의 개수만큼 사용자 정의 속성을 저장합니다.
                        {
                            PropTable pm = propTables[i];                            // 사용자 정의 속성을 가져옵니다.

                            writer.Write(pm.index);                                  // 사용자 정의 속성의 Index를 저장합니다.
                            writer.Write(pm.propCnt);                                // 사용자 정의 속성의 전체 개수를 저장합니다.

                            for (int j = 0; j < pm.propCnt; j++)                     // 속성의 개수만큼 Key, Value, Type값을 저장합니다.
                            {
                                // UTF-8로 변환
                                byte[] utf8BytesKey = Encoding.UTF8.GetBytes(pm.keyStr[j]);  // 속성의 Key값을 UTF-8 형식으로 변환합니다.
                                ulong size = (ulong)utf8BytesKey.Length;                     // 변경된 UTF-8 형식의 문자열의 크기를 가져옵니다.

                                // UTF-8 문자열 크기 기록
                                writer.Write(size);                                          // UTF-8 문자열의 크기를 저장합니다.

                                // UTF-8 문자열 데이터 기록
                                if (size > 0)                                                
                                {
                                    writer.Write(utf8BytesKey);                              // 문자열의 크기가 0보다 클경우에만 UTF-8 문자열을 저장합니다.
                                }

                                byte[] utf8BytesVal = Encoding.UTF8.GetBytes(pm.valStr[j]);  // 속성의 Value값을 UTF-8 형식으로 변환합니다.
                                size = (ulong)utf8BytesVal.Length;                           // 변경된 UTF-8 형식의 문자열의 크기를 가져옵니다.

                                // UTF-8 문자열 크기 기록
                                writer.Write(size);                                          // UTF-8 문자열의 크기를 저장합니다.

                                // UTF-8 문자열 데이터 기록
                                if (size > 0)
                                {
                                    writer.Write(utf8BytesVal);                              // 문자열의 크기가 0보다 클경우에만 UTF-8 문자열을 저장합니다.
                                }

                                writer.Write(pm.valType[j]);                                 // 현재 속성의 Type값을 저장합니다. 속성은 기본적으로 STRING객체이므로 TYPE은 0을 저장한다
                            }
                        }


                        ///MESH TABLE
                        int meshCnt = meshTables.Count();                            
                        writer.Write(meshCnt);                                       // 전체 NODE별 MESH 관리 테이블의 개수를 저장합니다.
                        for (int i = 0; i < meshCnt; i++)                            // MESH 관리 테이블의 개수만큼 MESH 정보를 저장합니다.
                        { 
                            MeshTable m = meshTables[i];                             // 관리 테이블에서 MESH 정보 구조체 1개를 가져옵니다.

                            writer.Write(m.index);                                   // 현재 MESH의 Index를 저장합니다.
                            for (int j = 0; j < 4; j++)                              // 현재 MESH의 색상 RGBA 값을 저장합니다.
                                writer.Write(m.color[j]);

                            int vnSize = m.VArray.Count();                           // 현재 MESH의 Vertex List의 개수를 가져옵니다.
                            m.vnSize = (uint)(vnSize / 3);                           // (x,y,z)가 1개의 Vertex(Triangle)이기 때문에 3으로 나눈 값을 저장합니다.                          
                            writer.Write(m.vnSize);
                            writer.Write(m.triSize);                                 // 전체 Triangle Index의 개수를 저장합니다.

                            if (m.vnSize > 0)                                        // MESH정보에 Vertex개수가 0이상일 경우에만 저장합니다.
                            {
                                for (int j = 0; j < vnSize; j++)                     // Vertex 개수만큼 Vertex(x,y,z) 값을 저장합니다.
                                {
                                    writer.Write(m.VArray[j]);
                                }

                                for (int j = 0; j < vnSize; j++)                     // Vertex 개수만큼 Normal(x,y,z) 값을 저장합니다.
                                {
                                    writer.Write(m.NArray[j]);
                                }
                            }

                            if (m.triSize > 0)                                       // Triangle Index의 개수가 0이상일 경우에만 저장합니다.
                            {
                                for (int j = 0; j < m.triSize; j++)                  // Triangle Index 개수만큼 Triangle Index (Vertex 순서) 값을 저장합니다.
                                {
                                    writer.Write(m.TriArray[j]);
                                }
                            }
                        }

                        writer.Close();                                              // BinaryWriter를 닫습니다.
                    }

                    fs.Close();                                                      // 파일을 닫습니다.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WriteBinaryFile Error: {ex.Message}");
            }

            Application.Exit();                                                      // 현재 응용프로그램을 종료합니다.
        }




        private void SetVIZStructureData(VIZCore3D.NET.Data.Node node, Structure st_p)
        {
            st_p.childNodeIDList.Add(struct_count); //부모 구조정보의 자식 NODE Index를 관리하는 리스트에 현재 구조정보의 Index값을 저장한다.
                                                    //HMF 파일 READ시 부모 자식 관계를 정의하기 위해 사용한다.

            // 중립 포멧 Structure 생성 및 초기화
            Structure structure = new Structure();

            // 현재 NODE의 인덱스
            structure.index = (uint)node.Index;

            uint ptype = 0;

            if (node.GetParent() != null)
                ptype = (uint)node.GetParent().Kind;
            uint type = (uint)node.Kind;

            structure.pIndex = node.ParentIndex;                 //현재 NODE의 부모 NODE 인덱스 값
            structure.ptype = ptype;                             //현재 NODE의 부모 NODE의 TYPE 정보 => ASSEMBLY(0), PART(1), BODY(2), BODYSET(3)
            structure.index = (uint)node.Index;                  //현재 NODE의 인덱스 값
            structure.type = type;                               //현재 NODE의 TYPE 정보 => ASSEMBLY(0), PART(1), BODY(2), BODYSET(3)
            structure.szName = node.NodeName;                    //현재 NODE의 이름
            structure.bBox = node.GetBoundBox().ToArray();       //현재 NODE의 BOUNDBOX 정보 MIN, MAX 값

            Matrix3D matrix = new Matrix3D();
            matrix.Identity();
            structure.transform = matrix.ToArray();              //현재 NODE의 MATRIX 정보 이미 Mesh에 적용되어 있어 Identity Matrix정보로 대체


            structure.propIDListCnt = (uint)node.GetUDA().Count; //현재 NODE의 사용자 정의 속성 개수

            if (structure.propIDListCnt > 0)                     //NODE에 속성이 있을 경우에만 처리한다.
            {
                structure.propIDList = new List<uint>();         //사용자 정의 속성을 저장할 LIST 초기화

                Dictionary<string, string> uda = node.GetUDA();  //현재 NODE의 사용자 정의 속성을 가져온다. <Key, Value>

                structure.propIDList.Add(prop_table_count);      //전체 사용자 정의 속성 LIST의 Index값을 저장한다. => propTables의 Index 정보
                PropTable pt_p = new PropTable();                //현재 NODE의 사용자 정의 속성을 담을 구조체를 초기화한다.
                pt_p.index = prop_table_count;                   //현재 NODE 속성의 Index값을 부여한다.
                pt_p.propCnt = structure.propIDListCnt;          //현재 NODE 속성의 전체 개수를 저장한다.

                foreach (KeyValuePair<string, string> item in uda) //현재 NODE 속성의 개수만큼 반복한다.
                {
                    pt_p.keyStr.Add(item.Key);                     //속성의 KEY값을 저장한다.
                    pt_p.valStr.Add(item.Value);                   //속성의 VALUE값을 저장한다.
                    pt_p.valType.Add(0);                           //속성은 기본적으로 STRING객체이므로 TYPE은 0을 저장한다.
                }

                pt_p.propCnt = (uint)pt_p.keyStr.Count;            //NODE에서 추출한 전체 속성개수와 실제 저장한 속성 개수가 상이할 수 있을 경우를 대비하여
                                                                   //실제 저장한 속성의 개수로 처리되도록 보정한다.

                propTables.Add(pt_p);                              //현재 NODE의 속성을 담은 구조체를 전체 속성 TABLE에 저장한다.
                prop_table_count++;                                //다음 NODE의 속성 Index를 부여하기 위해 전체 속성 Index관리 변수의 값을 +1한다. 
            }


            if (structure.type == 2)                               //NODE의 TYPE이 형상이 있는 BODY일 경우에만 처리되도록 한다.
            {
                List<Facet> facets = node.GetBodyFacet();          //현재 NODE의 VERTEX, NORMAL, TRIANGLE INDEX, COLOR 정보를 FACET 구조체로 가져온다.

                int facetCnt = facets.Count;                       //현재 NODE의 TRIANGLE SET 개수를 가져온다.
                structure.meshIDListCnt = (uint)facetCnt;          //현재 NODE의 TRIANGLE SET 개수를 중립포멧구조체에 저장한다.
                structure.meshIDList = new List<uint>();           //현재 NODE의 MESH정보를 담을 중립포멧 MESH구조체 LIST를 초기화한다.

                for (int m = 0; m < facetCnt; m++)                 //TRIANGLE SET 개수만큼 반복하여 처리한다.
                {
                    structure.meshIDList.Add(mesh_table_count);    //NODE MESHID관리 구조체에 현재 TRIANGLE SET의 Index를 추가한다.
                    MeshTable mt = new MeshTable();                //MESH 정보를 담을 구조체를 초기화한다.
                    mt.index = mesh_table_count;                   //MESH 정보의 index를 부여한다.
                    mesh_table_count++;                            //다음 MESH정보 index관리를 위해 전체 MESH Index관리 변수를 +1한다.

                    int vCnt = facets[m].Vertexes.Count;           //TRIANGLE SET의 Vertex 개수를 가져온다.
                    for (int v = 0; v < vCnt; v++)                 //Vertex개수 만큼 반복하며, Vertex / Normal 값을 List에 저장한다.
                    {
                        mt.VArray.Add(facets[m].Vertexes[v]);
                        mt.NArray.Add(facets[m].Normals[v]);
                    }

                    int tCnt = facets[m].TirIndexes.Count;         //TRIANGLE SET의 Triangle Index 개수를 가져온다.
                    for (int v = 0; v < tCnt; v++)                 //Triangle Index 개수만큼 반복하며 Triangle Index값을 List에 저장한다.
                        mt.TriArray.Add((ushort)facets[m].TirIndexes[v]);

                    mt.vnSize = (uint)(vCnt / 3);                  //Vertex/Normal의 개수를 저장한다. (x,y,z)가 1개의 Vertex이기 때문에 3으로 나눈 값을 저장한다.
                    mt.triSize = (uint)tCnt;                       //Triangle Index의 전체 개수를 저장한다.

                    // TRIANGLE SET의 RGBA 색상 정보를 저장한다. 
                    mt.color[0] = facets[m].Colors[0]; //RED
                    mt.color[1] = facets[m].Colors[1]; //BLUE
                    mt.color[2] = facets[m].Colors[2]; //GREEN
                    mt.color[3] = facets[m].Colors[3]; //ALPHA

                    meshTables.Add(mt);                            //현재 TRIANGLE SET MESH 정보를 전체 MESH 정보 관리 리스트에 저장한다.
                }
            }

            struct_count++;  //전체 structures에서 자식 NODE를 찾기 휘한 Index를 정의하기 위해 +1한다. 

            List<VIZCore3D.NET.Data.Node> children = node.GetChildObject3d(VIZCore3D.NET.Data.Object3DChildOption.CHILD_ONLY, true); //현재 NODE의 자식 NODE를 가져온다.
            int childCnt = children.Count;                    //자식 NODE의 개수를 가져온다.
            if (childCnt > 0)
                structure.childNodeIDList = new List<uint>(); //자식 NODE가 있을 경우, 중립포멧 구조체 자식NODE의 Index를 관리할 LIST를 초기화한다.

            for (int m = 0; m < childCnt; m++)                //자식 NODE의 개수만큼 반복한다.
            {
                VIZCore3D.NET.Data.Node cnode = children[m];  //자식 NODE리스트에서 1개의 NODE를 가져온다.
                SetVIZStructureData(cnode, structure);        //가져온 NODE의 정보를 중립포멧 구조체에 저장하기 위해 API를 호출한다.
            }

            structure.childNodeIDListCnt = (uint)childCnt;    //현재 구조 정보에 자식 NODE의 개수를 저장한다.

            structures.Add(structure);                        //전체 중립포멧 구조를 관리하는 LIST에 현재 정의한 구조 정보를 저장한다.
        }



        private void Model_OnModelProgressChangedEvent(object sender, VIZCore3D.NET.Event.EventManager.ModelProgressEventArgs e)
        {
            // string Mode = String.Empty;
            // int progress = e.Progress;
            // switch (e.Mode)
            // {
            //     case VIZCore3D.NET.Data.ModelProgressModes.READ:
            //         Mode = "파일 읽기 (열기)...";
            //         break;
            // }
            // System.Diagnostics.Trace.WriteLine(string.Format("{0} - Progress : {1} %", Mode, e.Progress));
        }
    }
}
