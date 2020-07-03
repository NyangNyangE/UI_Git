using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PanicCall
{
    [Serializable]
    public class Group : System.Runtime.Serialization.ISerializable
    {
        public int cars;
        public int areas;
        public int selectedIndex;
        public string selectedGroupName = "";
        public string groupName = "";
        public List<int> panicAddrs = new List<int>(); //저장용
        public List<PanicControl> panics; 
        public Group()
        {
            this.cars = 0; this.areas = 0; this.panicAddrs = new List<int>(); panics = new List<PanicControl>(); 
        }
        public Group(SerializationInfo info, StreamingContext context)
        {
            try
            {
                this.groupName = (string)info.GetValue("GroupName", groupName.GetType());
                this.selectedIndex = (int)info.GetValue("SelectedIndex", selectedIndex.GetType());
                this.selectedGroupName = (string)info.GetValue("SelectedGroupName", selectedGroupName.GetType());
                this.panicAddrs = (List<int>)info.GetValue("PanicAddrs", panicAddrs.GetType());
                //있을지 모르는 중복된 panic 제거 
                panicAddrs = panicAddrs.Distinct().ToList();
                this.panics = new List<PanicControl>();
                LoadPanics();

            }
            catch (Exception ex)
            {
                Console.Write("Group loading error : " + ex.Message);
            }
        }
        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue("GroupName", this.groupName);
            info.AddValue("SelectedIndex", this.selectedIndex);
            info.AddValue("SelectedGroupName", this.selectedGroupName);
            info.AddValue("PanicAddrs", this.panicAddrs);
            info.AddValue("Panics", this.panics);
        }
       
        // *********************************************************************************************
        //group 가 panic들을 저장할 수 없음 : panic 가 참조로 저장되기때문에 새로 시작시 주소값이 달라지게됨
        //따라서 panic addr 로 저장해두었다가 따로 panic 로드 
        // *********************************************************************************************

        /// <summary>
        /// Group 의 버튼리스트 panics 초기화, 로드
        /// </summary>
        public void LoadPanics()
        {
            panics.Clear();
            foreach (int addr in panicAddrs)
                foreach (Map map in MainWindow.maps)
                    if (map.PanicList.TryGetValue(addr, out PanicControl panic))
                        panics.Add(panic);
            
        }

        /// <summary>
        /// panics 에 panic 추가
        /// </summary>
        /// <param name="panic"></param>
        public void AddPanic(PanicControl panic)
        {
            panicAddrs.Add(panic.Addr);         
            panics.Add(panic);
        }

        /// <summary>
        /// panics 에서 panic 제거
        /// </summary>
        /// <param name="panic"></param>
        public void RemovePanic(PanicControl panic)
        {
            panicAddrs.Remove(panic.Addr);
            panics.Remove(panic);
        }

        /// <summary>
        /// panics 에서 모든 panic 제거
        /// </summary>
        public void RemoveAllPanic()
        {
            panicAddrs.Clear();
            panics.Clear();
        }

        /// <summary>
        /// 해당 그룹에 속해있는 panic들의 car, areas 의 수를 다시 계산
        /// </summary>
        public void CountCars()
        {
            cars = 0; areas = 0;
            foreach (PanicControl panic in panics)
            {
                foreach (Camera camera in panic.cameras)
                {
                    cars += camera.cars; areas += camera.areas;
                }
            }
        }
        
    }
}
