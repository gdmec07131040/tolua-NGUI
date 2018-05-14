-- @author 
-- @date 2018年5月14日
BaseView = BaseView or BaseClass()
function BaseView:__init()
    self.root_node = nil
    self.node_list = {}
end
function BaseView:__delete()
end
--TODO lua
--1.使用resource_manager加载UI界面
--2.每个界面有唯一的标志
--3.界面打开类型标志 是销毁还是隐藏
--4.存储界面子对象避免直接使用Find
--5.如果使用界面动画 需要一个父对象
--TODO c#
--1.LuaObjectPool
--2.GameObjectList