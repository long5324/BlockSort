using System;
using UnityEngine;

public class ProjectReview
{
    //Review Lần 1
    //- Sẽ Update game theo dòng HexaSort : https://www.youtube.com/playlist?list=PLtE3VBsI95cKWHeQyEwykYC8zDLD7uIKK
    
    //*Unity
    //- Cẩn thận hơn trong việc đặt vị trí tất cả các object trong scene 
    //    (hiện tại một lượng lớn object đang có vị trí không dựa theo một base nào..., các panel trong canvas có trục z sai)
    //- Collider size của BlockControl cần to hơn nữa, offset khi bấm cần cao hơn 
    //- Kinh nghiệm : Không nên dùng bản Unity 6 bản mới và render URP như hiện tại, có nhiều setting và input khó kiểm soát. Nên dùng Build-In

    //*Code
    //- Phân chia Code manger và các object manager nhỏ hơn nữa, hiện tại đang để hết script manager và component liên quan vào hết Gamemanager
    //- Không nên code logic core gameplay vào GameManager mà nên tạo 1 class là GameplayManager để làm chuyện này, về sau game có nhiều level sẽ dễ xử lí hơn;
    //- Cần clean code từ cách đặt tên biến đến đặt tên hàm, hiện có một số hàm đặt tên theo kiểu : setPause()
    //- Class Block Control đang khá khó hiểu : Nó vửa là 3 khối ở trên để sinh ra, vừa là các Grid ở dưới (Không nên như vậy vì nhiệm vụ của đám này khác nhau)
    //- Nên chia nhỏ code hơn nữa: ChildBlock nên là class Monobehavior
    //- Đang có vấn đề về Cache Component
    //- Dùng Layermask bằng seriallize, không nên dùng string
    //- Có 1 class Constain để lưu string
    //- Một hàm sử dụng trong Update chưa ổn lắm (gây hại cho hiệu năng)
    //- Học cách sử dụng thêm Design Pattern : Object Pool, State Machine (cơ bản), Observer pattern (cơ bản) để code có modul và rõ ràng hơn.


}
