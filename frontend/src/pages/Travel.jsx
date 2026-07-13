import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Plus, X, Plane, DollarSign, MapPin, AlertCircle, CheckCircle, Clock, Calendar } from 'lucide-react';
import api from '../services/api';
import './Travel.css';

const Travel = () => {
  const { user } = useAuth();
  const [travelOrders, setTravelOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  // Submit Travel states
  const [isOpen, setIsOpen] = useState(false);
  const [description, setDescription] = useState('');
  const [destination, setDestination] = useState('');
  const [costs, setCosts] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  useEffect(() => {
    fetchTravelOrders();
  }, [user]);

  const fetchTravelOrders = async () => {
    try {
      setLoading(true);
      setError('');
      let rawRequests = [];
      if (user.role === 'Admin') {
        const res = await api.get('/requests');
        rawRequests = res.data;
      } else if (user.role === 'Manager') {
        const [reqRes, empRes] = await Promise.all([
          api.get('/requests'),
          api.get('/employees')
        ]);
        
        const managerEmp = empRes.data.find(e => e.id === user.id);
        const managerDept = managerEmp?.department;
        
        rawRequests = reqRes.data.filter(r => {
          if (r.employeeId === user.id) return true;
          
          const emp = empRes.data.find(e => e.id === r.employeeId);
          if (!emp) return false;
          
          if (emp.role === 'Admin') return false;
          if (emp.role === 'Manager' && emp.id !== user.id) return false;
          
          return (emp.department === managerDept || emp.managerId === user.id);
        });
      } else {
        const res = await api.get(`/requests/employee/${user.id}`);
        rawRequests = res.data;
      }
      
      // Filter requests of type TravelOrder (enum value 2 or string 'TravelOrder')
      const filtered = rawRequests.filter(r => r.type === 2 || r.type === 'TravelOrder');
      setTravelOrders(filtered);
    } catch (err) {
      console.error("Failed to load travel orders", err);
      setError("Failed to load travel orders from server.");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!description.trim() || !destination.trim() || !costs || !startDate || !endDate) {
      setError("All fields are required.");
      return;
    }

    try {
      // Submit travel request via Ocelot Gateway to RequestService
      const res = await api.post('/requests/leave', {
        employeeId: user.id,
        description,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
        type: 2, // TravelOrder
        destination,
        costs: parseFloat(costs)
      });
      const reqId = res.data;

      if (user.role === 'Admin') {
        await api.put(`/requests/${reqId}/approve`, null, { headers: { 'X-Approver-Role': 'Manager' } });
        await api.put(`/requests/${reqId}/approve`, null, { headers: { 'X-Approver-Role': 'Admin' } });
      }

      if (user.role === 'Manager') {
        try {
          const empRes = await api.get('/employees');
          const admin = empRes.data.find(e => e.role === 'Admin');
          if (admin) {
            await api.post('/notifications', {
              recipientId: admin.id,
              message: `Manager ${user.name} has submitted a new request for Travel Order.`
            });
          }
        } catch (err) {
          console.error("Failed to notify admin about manager travel order", err);
        }
      }

      setIsOpen(false);
      setDescription('');
      setDestination('');
      setCosts('');
      setStartDate('');
      setEndDate('');
      fetchTravelOrders();
    } catch (err) {
      console.error("Travel order submission failed", err);
      setError(err.response?.data?.message || "Failed to submit travel request. Verify date availability.");
    }
  };

  const getStatusIcon = (status) => {
    switch (status) {
      case 'Approved':
        return <CheckCircle className="text-success" size={20} />;
      case 'Rejected':
        return <X className="text-danger" size={20} />;
      default:
        return <Clock className="text-warning" size={20} />;
    }
  };

  return (
    <div className="travel-container">
      <div className="page-header flex-header">
        <div>
          <h1>Travel Orders</h1>
          <p className="subtitle">Submit business trip requests and track your expenses.</p>
        </div>
        {(user.role === 'Employee' || user.role === 'Manager' || user.role === 'Admin') && (
          <button className="btn-primary" onClick={() => setIsOpen(true)}>
            <Plus size={18} />
            <span>New Travel Order</span>
          </button>
        )}
      </div>

      {error && <div className="error-banner flex-align-center gap-sm"><AlertCircle className="text-danger" /><span>{error}</span></div>}

      {loading ? (
        <div className="loader">Syncing travel database...</div>
      ) : (
        <div className="travel-grid">
          {travelOrders.map(order => (
            <div key={order.id} className="travel-card glass">
              <div className="travel-card-header">
                <Plane size={24} className="text-primary" />
                <div className="travel-status">
                  {getStatusIcon(order.status)}
                  <span className="status-text">{order.status}</span>
                </div>
              </div>

              <div className="travel-card-body">
                <h3>{order.description}</h3>
                <div className="detail-item mt-sm">
                  <MapPin size={16} className="text-muted" />
                  <span><strong>Destination:</strong> {order.travelOrder?.destination || 'N/A'}</span>
                </div>
                <div className="detail-item">
                  <DollarSign size={16} className="text-muted" />
                  <span><strong>Estimated Costs:</strong> ${order.travelOrder?.costs || 0}</span>
                </div>
                <div className="detail-item">
                  <Calendar size={16} className="text-muted" />
                  <span>{new Date(order.startDate).toLocaleDateString()} - {new Date(order.endDate).toLocaleDateString()}</span>
                </div>
              </div>
            </div>
          ))}
          {travelOrders.length === 0 && (
            <div className="empty-state-card glass text-center py-xl col-span-all">
              No business trip requests submitted.
            </div>
          )}
        </div>
      )}

      {/* New Travel Order Modal */}
      {isOpen && (
        <div className="modal-overlay">
          <div className="modal-card glass">
            <div className="modal-header">
              <h2>New Travel Order Request</h2>
              <button className="icon-btn" onClick={() => setIsOpen(false)}><X size={20} /></button>
            </div>
            
            <form onSubmit={handleSubmit} className="modal-form">
              <div className="form-group">
                <label>Trip Description / Purpose</label>
                <input type="text" className="form-input" value={description} onChange={e => setDescription(e.target.value)} required placeholder="E.g., Annual Dev Conference, Client meeting" />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Destination</label>
                  <input type="text" className="form-input" value={destination} onChange={e => setDestination(e.target.value)} required placeholder="City, Country" />
                </div>
                <div className="form-group">
                  <label>Estimated Costs ($)</label>
                  <input type="number" className="form-input" value={costs} onChange={e => setCosts(e.target.value)} required placeholder="500" />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Start Date</label>
                  <input type="date" className="form-input" value={startDate} onChange={e => setStartDate(e.target.value)} required />
                </div>
                <div className="form-group">
                  <label>End Date</label>
                  <input type="date" className="form-input" value={endDate} onChange={e => setEndDate(e.target.value)} required />
                </div>
              </div>

              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={() => setIsOpen(false)}>Cancel</button>
                <button type="submit" className="btn-primary" disabled={!description.trim() || !destination.trim() || !costs || !startDate || !endDate}>Submit Request</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Travel;
