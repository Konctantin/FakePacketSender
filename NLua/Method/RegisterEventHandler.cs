using System;
using System.Reflection;

namespace NLua.Method
{
    /// <summary>
    /// Wrapper class for events that does registration/deregistration of event handlers.
    /// </summary>
    internal class RegisterEventHandler
    {
        private EventHandlerContainer pendingEvents;
        private EventInfo eventInfo;
        private object target;

        public RegisterEventHandler(EventHandlerContainer pendingEvents, object target, EventInfo eventInfo)
        {
            this.target = target;
            this.eventInfo = eventInfo;
            this.pendingEvents = pendingEvents;
        }

        /// <summary>
        /// Adds a new event handler
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public Delegate Add(LuaFunction function)
        {
            //CP: Fix by Ben Bryant for event handling with one parameter
            //link: http://luaforge.net/forum/message.php?msg_id=9266
            Delegate handlerDelegate = CodeGeneration.Instance.GetDelegate(eventInfo.EventHandlerType, function);
            eventInfo.AddEventHandler(target, handlerDelegate);
            pendingEvents.Add(handlerDelegate, this);

            return handlerDelegate;
        }

        /// <summary>
        /// Removes an existing event handler
        /// </summary>
        /// <param name="handlerDelegate"></param>
        public void Remove(Delegate handlerDelegate)
        {
            RemovePending(handlerDelegate);
            pendingEvents.Remove(handlerDelegate);
        }

        /// <summary>
        /// Removes an existing event handler (without updating the pending handlers list)
        /// </summary>
        /// <param name="handlerDelegate"></param>
        internal void RemovePending(Delegate handlerDelegate)
        {
            eventInfo.RemoveEventHandler(target, handlerDelegate);
        }
    }
}