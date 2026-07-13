import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Calendar as CalendarIcon, ChevronLeft, ChevronRight, Briefcase, BookOpen, Clock, Plane } from 'lucide-react';
import api from '../services/api';
import './Calendar.css';

const CalendarPage = () => {
  const { user } = useAuth();
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentDate, setCurrentDate] = useState(new Date());

  useEffect(() => {
    fetchEvents();
  }, [user]);

  const fetchEvents = async () => {
    try {
      setLoading(true);
      const res = await api.get(`/requests/employee/${user.id}`);
      
      // Filter out Rejected requests
      const activeRequests = res.data.filter(r => r.status !== 'Rejected');
      
      // Format as event objects
      const formatted = activeRequests.map(r => ({
        id: r.id,
        title: r.description,
        startDate: new Date(r.startDate),
        endDate: new Date(r.endDate),
        type: r.type, // 1 = Leave, 2 = Travel, 3 = Internship, 4 = Education
        status: r.status,
        leaveType: r.leaveType
      }));
      setEvents(formatted);
    } catch (err) {
      console.error("Failed to load calendar events", err);
    } finally {
      setLoading(false);
    }
  };

  const getDaysInMonth = (date) => {
    const year = date.getFullYear();
    const month = date.getMonth();
    return new Date(year, month + 1, 0).getDate();
  };

  const getFirstDayOfMonth = (date) => {
    const year = date.getFullYear();
    const month = date.getMonth();
    return new Date(year, month, 1).getDay(); // 0 = Sunday, 1 = Monday, etc.
  };

  const nextMonth = () => {
    setCurrentDate(new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1));
  };

  const prevMonth = () => {
    setCurrentDate(new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1));
  };

  const getEventsForDay = (day) => {
    const dateToCheck = new Date(currentDate.getFullYear(), currentDate.getMonth(), day);
    return events.filter(e => {
      // Reset times to check date boundaries only
      const start = new Date(e.startDate.getFullYear(), e.startDate.getMonth(), e.startDate.getDate());
      const end = new Date(e.endDate.getFullYear(), e.endDate.getMonth(), e.endDate.getDate());
      return dateToCheck >= start && dateToCheck <= end;
    });
  };

  const getEventClass = (type, leaveType) => {
    if (type === 1 || type === 'Leave') {
      if (leaveType === 2 || leaveType === 'SickLeave') return 'event-sick';
      if (leaveType === 3 || leaveType === 'DayOff') return 'event-dayoff';
      return 'event-leave';
    }
    if (type === 2 || type === 'TravelOrder') return 'event-travel';
    if (type === 3 || type === 'Internship') return 'event-internship';
    return 'event-education';
  };

  const getEventIcon = (type) => {
    if (type === 1 || type === 'Leave') {
      return <Clock size={12} />;
    }
    if (type === 2 || type === 'TravelOrder') return <Plane size={12} />;
    if (type === 3 || type === 'Internship') return <Briefcase size={12} />;
    return <BookOpen size={12} />;
  };

  const renderDays = () => {
    const daysInMonth = getDaysInMonth(currentDate);
    const firstDay = getFirstDayOfMonth(currentDate);
    const days = [];

    // Empty spots for preceding days
    // Adjusting Sunday index (Sunday = 0, we want Monday = 0 to make it European format)
    const adjustedFirstDay = firstDay === 0 ? 6 : firstDay - 1;
    for (let i = 0; i < adjustedFirstDay; i++) {
      days.push(<div key={`empty-${i}`} className="calendar-day empty"></div>);
    }

    // Days in current month
    for (let day = 1; day <= daysInMonth; day++) {
      const dayEvents = getEventsForDay(day);
      days.push(
        <div key={`day-${day}`} className="calendar-day glass">
          <span className="day-number">{day}</span>
          <div className="day-events">
            {dayEvents.map(e => (
              <div key={e.id} className={`day-event ${getEventClass(e.type, e.leaveType)}`} title={`${e.title} (${e.status})`}>
                {getEventIcon(e.type)}
                <span className="event-title-text">{e.title}</span>
              </div>
            ))}
          </div>
        </div>
      );
    }

    return days;
  };

  const monthNames = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
  ];

  const weekdayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

  return (
    <div className="calendar-container">
      <div className="page-header flex-header">
        <div>
          <h1>Personal Activity Calendar</h1>
          <p className="subtitle">View all your scheduled absences, travel orders, and educations in one place.</p>
        </div>
      </div>

      {loading ? (
        <div className="loader">Building your schedule...</div>
      ) : (
        <div className="calendar-card glass">
          <div className="calendar-header">
            <h2>{monthNames[currentDate.getMonth()]} {currentDate.getFullYear()}</h2>
            <div className="calendar-navigation">
              <button className="icon-btn" onClick={prevMonth}><ChevronLeft size={20} /></button>
              <button className="icon-btn" onClick={nextMonth}><ChevronRight size={20} /></button>
            </div>
          </div>

          {/* Weekday headers */}
          <div className="calendar-weekdays">
            {weekdayNames.map(w => (
              <div key={w} className="weekday-header">{w}</div>
            ))}
          </div>

          {/* Calendar grid */}
          <div className="calendar-days-grid">
            {renderDays()}
          </div>

          {/* Color coding legend */}
          <div className="calendar-legend mt-lg glass">
            <h4>Legend</h4>
            <div className="legend-items">
              <div className="legend-item"><span className="legend-color event-leave"></span><span>Annual Leave</span></div>
              <div className="legend-item"><span className="legend-color event-sick"></span><span>Sick Leave</span></div>
              <div className="legend-item"><span className="legend-color event-dayoff"></span><span>Day Off</span></div>
              <div className="legend-item"><span className="legend-color event-travel"></span><span>Travel Order</span></div>
              <div className="legend-item"><span className="legend-color event-internship"></span><span>Internship</span></div>
              <div className="legend-item"><span className="legend-color event-education"></span><span>Education</span></div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CalendarPage;
