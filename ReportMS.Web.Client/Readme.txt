﻿/* gang.yang 2016-01-10*/

/* ReportMS.Web.Client */
对当前的 ReportMS.Web 系统提供支持


Job 后台运行注意事项

在服务器端，设置 IIS，禁用应用程序池的回收机制
通过 IIS 面板操作（高级设置）

     回收列表：
          固定时间间隔(分钟) -- 更改为 0 （默认 1740）
          虚拟/专用内存限制（KB）-- 更改为 0
     进程模式：
          闲置超时(分钟) -- 改为 0 （默认 20）
