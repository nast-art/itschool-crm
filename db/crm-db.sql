--
-- PostgreSQL database dump
--

\restrict EDLGEmT8apZFj9eqSX8eO3dU3OjC87Xtcp9hMetYsLHRcFGZ9rqfuoTMgoNChTs

-- Dumped from database version 18.3
-- Dumped by pg_dump version 18.3

-- Started on 2026-09-25 22:09:34

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 250 (class 1259 OID 54805)
-- Name: attachments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.attachments (
    attachments_id integer NOT NULL,
    interaction_id integer,
    status_id integer,
    uploaded_by integer,
    file_name character varying(300),
    storage_path character varying(500),
    mime_type character varying(100),
    file_size bigint,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.attachments OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 54804)
-- Name: attachments_attachments_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.attachments_attachments_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.attachments_attachments_id_seq OWNER TO postgres;

--
-- TOC entry 5249 (class 0 OID 0)
-- Dependencies: 249
-- Name: attachments_attachments_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.attachments_attachments_id_seq OWNED BY public.attachments.attachments_id;


--
-- TOC entry 252 (class 1259 OID 54831)
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.audit_logs (
    audit_logs_id integer NOT NULL,
    user_id integer,
    action character varying(100),
    entity_type character varying(100),
    entity_id integer,
    old_data jsonb,
    new_data jsonb,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.audit_logs OWNER TO postgres;

--
-- TOC entry 251 (class 1259 OID 54830)
-- Name: audit_logs_audit_logs_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.audit_logs_audit_logs_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.audit_logs_audit_logs_id_seq OWNER TO postgres;

--
-- TOC entry 5250 (class 0 OID 0)
-- Dependencies: 251
-- Name: audit_logs_audit_logs_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.audit_logs_audit_logs_id_seq OWNED BY public.audit_logs.audit_logs_id;


--
-- TOC entry 230 (class 1259 OID 54585)
-- Name: contracts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.contracts (
    contracts_id integer NOT NULL,
    contract_number character varying(100),
    signed_at timestamp without time zone,
    status character varying(100),
    comment text,
    created_at timestamp without time zone DEFAULT now(),
    valid_until timestamp without time zone
);


ALTER TABLE public.contracts OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 54584)
-- Name: contracts_contracts_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.contracts_contracts_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.contracts_contracts_id_seq OWNER TO postgres;

--
-- TOC entry 5251 (class 0 OID 0)
-- Dependencies: 229
-- Name: contracts_contracts_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.contracts_contracts_id_seq OWNED BY public.contracts.contracts_id;


--
-- TOC entry 254 (class 1259 OID 54847)
-- Name: import_batches; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.import_batches (
    import_batches_id integer NOT NULL,
    file_name character varying(300),
    uploaded_by integer,
    status character varying(100),
    created_at timestamp without time zone DEFAULT now(),
    completed_at timestamp without time zone,
    error_message text
);


ALTER TABLE public.import_batches OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 54846)
-- Name: import_batches_import_batches_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.import_batches_import_batches_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.import_batches_import_batches_id_seq OWNER TO postgres;

--
-- TOC entry 5252 (class 0 OID 0)
-- Dependencies: 253
-- Name: import_batches_import_batches_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.import_batches_import_batches_id_seq OWNED BY public.import_batches.import_batches_id;


--
-- TOC entry 248 (class 1259 OID 54774)
-- Name: interaction_status_history; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.interaction_status_history (
    interaction_status_history_id integer CONSTRAINT interaction_status_history_interaction_status_history__not_null NOT NULL,
    interaction_id integer,
    from_status_id integer,
    to_status_id integer,
    changed_by integer,
    comment text,
    changed_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.interaction_status_history OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 54773)
-- Name: interaction_status_history_interaction_status_history_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.interaction_status_history_interaction_status_history_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.interaction_status_history_interaction_status_history_id_seq OWNER TO postgres;

--
-- TOC entry 5253 (class 0 OID 0)
-- Dependencies: 247
-- Name: interaction_status_history_interaction_status_history_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.interaction_status_history_interaction_status_history_id_seq OWNED BY public.interaction_status_history.interaction_status_history_id;


--
-- TOC entry 246 (class 1259 OID 54720)
-- Name: interactions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.interactions (
    interactions_id integer NOT NULL,
    university_id integer,
    program_id integer,
    product_id integer,
    manager_id integer,
    university_contact_id integer,
    workflow_id integer,
    current_status_id integer,
    contract_id integer,
    license_id integer,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone
);


ALTER TABLE public.interactions OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 54719)
-- Name: interactions_interactions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.interactions_interactions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.interactions_interactions_id_seq OWNER TO postgres;

--
-- TOC entry 5254 (class 0 OID 0)
-- Dependencies: 245
-- Name: interactions_interactions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.interactions_interactions_id_seq OWNED BY public.interactions.interactions_id;


--
-- TOC entry 220 (class 1259 OID 54515)
-- Name: it_directions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.it_directions (
    it_directions_id integer NOT NULL,
    name character varying(200),
    description text,
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone,
    priority integer
);


ALTER TABLE public.it_directions OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 54514)
-- Name: it_directions_it_directions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.it_directions_it_directions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.it_directions_it_directions_id_seq OWNER TO postgres;

--
-- TOC entry 5255 (class 0 OID 0)
-- Dependencies: 219
-- Name: it_directions_it_directions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.it_directions_it_directions_id_seq OWNED BY public.it_directions.it_directions_id;


--
-- TOC entry 224 (class 1259 OID 54541)
-- Name: it_products; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.it_products (
    it_products_id integer NOT NULL,
    name character varying(200),
    vendor character varying(200),
    description text,
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone
);


ALTER TABLE public.it_products OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 54540)
-- Name: it_products_it_products_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.it_products_it_products_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.it_products_it_products_id_seq OWNER TO postgres;

--
-- TOC entry 5256 (class 0 OID 0)
-- Dependencies: 223
-- Name: it_products_it_products_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.it_products_it_products_id_seq OWNED BY public.it_products.it_products_id;


--
-- TOC entry 234 (class 1259 OID 54609)
-- Name: it_programs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.it_programs (
    it_programs_id integer NOT NULL,
    direction_id integer,
    name character varying(200),
    description text,
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone
);


ALTER TABLE public.it_programs OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 54608)
-- Name: it_programs_it_programs_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.it_programs_it_programs_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.it_programs_it_programs_id_seq OWNER TO postgres;

--
-- TOC entry 5257 (class 0 OID 0)
-- Dependencies: 233
-- Name: it_programs_it_programs_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.it_programs_it_programs_id_seq OWNED BY public.it_programs.it_programs_id;


--
-- TOC entry 232 (class 1259 OID 54598)
-- Name: licenses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.licenses (
    licenses_id integer NOT NULL,
    signed_at timestamp without time zone,
    valid_until timestamp without time zone,
    transfer_status character varying(100),
    comment text,
    created_at timestamp without time zone DEFAULT now()
);


ALTER TABLE public.licenses OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 54597)
-- Name: licenses_licenses_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.licenses_licenses_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.licenses_licenses_id_seq OWNER TO postgres;

--
-- TOC entry 5258 (class 0 OID 0)
-- Dependencies: 231
-- Name: licenses_licenses_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.licenses_licenses_id_seq OWNED BY public.licenses.licenses_id;


--
-- TOC entry 242 (class 1259 OID 54679)
-- Name: program_products; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.program_products (
    program_products_id integer NOT NULL,
    program_id integer,
    product_id integer
);


ALTER TABLE public.program_products OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 54678)
-- Name: program_products_program_products_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.program_products_program_products_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.program_products_program_products_id_seq OWNER TO postgres;

--
-- TOC entry 5259 (class 0 OID 0)
-- Dependencies: 241
-- Name: program_products_program_products_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.program_products_program_products_id_seq OWNED BY public.program_products.program_products_id;


--
-- TOC entry 222 (class 1259 OID 54529)
-- Name: universities; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.universities (
    universities_id integer NOT NULL,
    name character varying(300),
    short_name character varying(100),
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone
);


ALTER TABLE public.universities OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 54528)
-- Name: universities_universities_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.universities_universities_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.universities_universities_id_seq OWNER TO postgres;

--
-- TOC entry 5260 (class 0 OID 0)
-- Dependencies: 221
-- Name: universities_universities_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.universities_universities_id_seq OWNED BY public.universities.universities_id;


--
-- TOC entry 238 (class 1259 OID 54643)
-- Name: university_contacts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.university_contacts (
    university_contacts_id integer NOT NULL,
    university_id integer,
    full_name character varying(200),
    "position" character varying(200),
    email character varying(200),
    phone character varying(50),
    is_active boolean DEFAULT true,
    comment text
);


ALTER TABLE public.university_contacts OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 54642)
-- Name: university_contacts_university_contacts_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.university_contacts_university_contacts_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.university_contacts_university_contacts_id_seq OWNER TO postgres;

--
-- TOC entry 5261 (class 0 OID 0)
-- Dependencies: 237
-- Name: university_contacts_university_contacts_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.university_contacts_university_contacts_id_seq OWNED BY public.university_contacts.university_contacts_id;


--
-- TOC entry 240 (class 1259 OID 54659)
-- Name: university_managers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.university_managers (
    university_managers_id integer NOT NULL,
    university_id integer,
    user_id integer,
    assigned_at timestamp without time zone DEFAULT now(),
    is_primary boolean DEFAULT false
);


ALTER TABLE public.university_managers OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 54658)
-- Name: university_managers_university_managers_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.university_managers_university_managers_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.university_managers_university_managers_id_seq OWNER TO postgres;

--
-- TOC entry 5262 (class 0 OID 0)
-- Dependencies: 239
-- Name: university_managers_university_managers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.university_managers_university_managers_id_seq OWNED BY public.university_managers.university_managers_id;


--
-- TOC entry 226 (class 1259 OID 54555)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    users_id integer NOT NULL,
    keycloak_user_id character varying(200),
    email character varying(200),
    is_active boolean DEFAULT true,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone,
    last_name character varying(100),
    first_name character varying(100),
    middle_name character varying(100)
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 54554)
-- Name: users_users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_users_id_seq OWNER TO postgres;

--
-- TOC entry 5263 (class 0 OID 0)
-- Dependencies: 225
-- Name: users_users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_users_id_seq OWNED BY public.users.users_id;


--
-- TOC entry 236 (class 1259 OID 54626)
-- Name: workflow_statuses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.workflow_statuses (
    workflow_statuses_id integer NOT NULL,
    workflow_id integer,
    name character varying(200),
    description text,
    sort_order integer,
    is_initial boolean DEFAULT false,
    is_final boolean DEFAULT false
);


ALTER TABLE public.workflow_statuses OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 54625)
-- Name: workflow_statuses_workflow_statuses_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.workflow_statuses_workflow_statuses_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.workflow_statuses_workflow_statuses_id_seq OWNER TO postgres;

--
-- TOC entry 5264 (class 0 OID 0)
-- Dependencies: 235
-- Name: workflow_statuses_workflow_statuses_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.workflow_statuses_workflow_statuses_id_seq OWNED BY public.workflow_statuses.workflow_statuses_id;


--
-- TOC entry 244 (class 1259 OID 54697)
-- Name: workflow_transitions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.workflow_transitions (
    workflow_transitions_id integer NOT NULL,
    workflow_id integer,
    from_status_id integer,
    to_status_id integer
);


ALTER TABLE public.workflow_transitions OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 54696)
-- Name: workflow_transitions_workflow_transitions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.workflow_transitions_workflow_transitions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.workflow_transitions_workflow_transitions_id_seq OWNER TO postgres;

--
-- TOC entry 5265 (class 0 OID 0)
-- Dependencies: 243
-- Name: workflow_transitions_workflow_transitions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.workflow_transitions_workflow_transitions_id_seq OWNED BY public.workflow_transitions.workflow_transitions_id;


--
-- TOC entry 228 (class 1259 OID 54571)
-- Name: workflows; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.workflows (
    workflows_id integer NOT NULL,
    name character varying(200),
    description text,
    is_active boolean DEFAULT true,
    version integer,
    created_at timestamp without time zone DEFAULT now(),
    updated_at timestamp without time zone
);


ALTER TABLE public.workflows OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 54570)
-- Name: workflows_workflows_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.workflows_workflows_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.workflows_workflows_id_seq OWNER TO postgres;

--
-- TOC entry 5266 (class 0 OID 0)
-- Dependencies: 227
-- Name: workflows_workflows_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.workflows_workflows_id_seq OWNED BY public.workflows.workflows_id;


--
-- TOC entry 4977 (class 2604 OID 54808)
-- Name: attachments attachments_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attachments ALTER COLUMN attachments_id SET DEFAULT nextval('public.attachments_attachments_id_seq'::regclass);


--
-- TOC entry 4979 (class 2604 OID 54834)
-- Name: audit_logs audit_logs_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs ALTER COLUMN audit_logs_id SET DEFAULT nextval('public.audit_logs_audit_logs_id_seq'::regclass);


--
-- TOC entry 4956 (class 2604 OID 54588)
-- Name: contracts contracts_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.contracts ALTER COLUMN contracts_id SET DEFAULT nextval('public.contracts_contracts_id_seq'::regclass);


--
-- TOC entry 4981 (class 2604 OID 54850)
-- Name: import_batches import_batches_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.import_batches ALTER COLUMN import_batches_id SET DEFAULT nextval('public.import_batches_import_batches_id_seq'::regclass);


--
-- TOC entry 4975 (class 2604 OID 54777)
-- Name: interaction_status_history interaction_status_history_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interaction_status_history ALTER COLUMN interaction_status_history_id SET DEFAULT nextval('public.interaction_status_history_interaction_status_history_id_seq'::regclass);


--
-- TOC entry 4973 (class 2604 OID 54723)
-- Name: interactions interactions_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions ALTER COLUMN interactions_id SET DEFAULT nextval('public.interactions_interactions_id_seq'::regclass);


--
-- TOC entry 4941 (class 2604 OID 54518)
-- Name: it_directions it_directions_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.it_directions ALTER COLUMN it_directions_id SET DEFAULT nextval('public.it_directions_it_directions_id_seq'::regclass);


--
-- TOC entry 4947 (class 2604 OID 54544)
-- Name: it_products it_products_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.it_products ALTER COLUMN it_products_id SET DEFAULT nextval('public.it_products_it_products_id_seq'::regclass);


--
-- TOC entry 4960 (class 2604 OID 54612)
-- Name: it_programs it_programs_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.it_programs ALTER COLUMN it_programs_id SET DEFAULT nextval('public.it_programs_it_programs_id_seq'::regclass);


--
-- TOC entry 4958 (class 2604 OID 54601)
-- Name: licenses licenses_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.licenses ALTER COLUMN licenses_id SET DEFAULT nextval('public.licenses_licenses_id_seq'::regclass);


--
-- TOC entry 4971 (class 2604 OID 54682)
-- Name: program_products program_products_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.program_products ALTER COLUMN program_products_id SET DEFAULT nextval('public.program_products_program_products_id_seq'::regclass);


--
-- TOC entry 4944 (class 2604 OID 54532)
-- Name: universities universities_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.universities ALTER COLUMN universities_id SET DEFAULT nextval('public.universities_universities_id_seq'::regclass);


--
-- TOC entry 4966 (class 2604 OID 54646)
-- Name: university_contacts university_contacts_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.university_contacts ALTER COLUMN university_contacts_id SET DEFAULT nextval('public.university_contacts_university_contacts_id_seq'::regclass);


--
-- TOC entry 4968 (class 2604 OID 54662)
-- Name: university_managers university_managers_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.university_managers ALTER COLUMN university_managers_id SET DEFAULT nextval('public.university_managers_university_managers_id_seq'::regclass);


--
-- TOC entry 4950 (class 2604 OID 54558)
-- Name: users users_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN users_id SET DEFAULT nextval('public.users_users_id_seq'::regclass);


--
-- TOC entry 4963 (class 2604 OID 54629)
-- Name: workflow_statuses workflow_statuses_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflow_statuses ALTER COLUMN workflow_statuses_id SET DEFAULT nextval('public.workflow_statuses_workflow_statuses_id_seq'::regclass);


--
-- TOC entry 4972 (class 2604 OID 54700)
-- Name: workflow_transitions workflow_transitions_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflow_transitions ALTER COLUMN workflow_transitions_id SET DEFAULT nextval('public.workflow_transitions_workflow_transitions_id_seq'::regclass);


--
-- TOC entry 4953 (class 2604 OID 54574)
-- Name: workflows workflows_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflows ALTER COLUMN workflows_id SET DEFAULT nextval('public.workflows_workflows_id_seq'::regclass);


--
-- TOC entry 5239 (class 0 OID 54805)
-- Dependencies: 250
-- Data for Name: attachments; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.attachments (attachments_id, interaction_id, status_id, uploaded_by, file_name, storage_path, mime_type, file_size, created_at) VALUES (1, 94, 1, 1, 'report_20260919_102504.xlsx', 'uploads/attachments/94/1/5a48922248654226b7fb55b4798a208b_report_20260919_102504.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 7154, '2026-09-19 13:42:28.198792');
INSERT INTO public.attachments (attachments_id, interaction_id, status_id, uploaded_by, file_name, storage_path, mime_type, file_size, created_at) VALUES (3, 24, 1, 7, 'Хакатон-1.zip', 'uploads/attachments/24/1/6e9f3acc034b4b23a23ecac476fbdb9f_Хакатон-1.zip', 'application/zip', 18554, '2026-09-25 15:59:16.598327');
INSERT INTO public.attachments (attachments_id, interaction_id, status_id, uploaded_by, file_name, storage_path, mime_type, file_size, created_at) VALUES (4, 24, 1, 7, 'access-user.jpg', 'uploads/attachments/24/1/3a9bdb15e33b4b9fa6c89d4e0efa3fbd_access-user.jpg', 'image/jpeg', 65095, '2026-09-25 18:50:45.580558');


--
-- TOC entry 5241 (class 0 OID 54831)
-- Dependencies: 252
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (1, 1, 'STATUS_CHANGE', 'interaction', 94, '{"StatusId": 10, "StatusName": "Актуализация учебной программы"}', '{"Comment": "Актуализировали учебную программу", "StatusId": 11, "StatusName": "Ведение занятий"}', '2026-09-19 13:42:01.749099');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (2, 1, 'UPLOAD', 'attachment', 1, NULL, '{"file_name": "report_20260919_102504.xlsx", "file_size": 7154, "mime_type": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "status_id": 1, "uploaded_by": 1, "attachments_id": 1, "interaction_id": 94}', '2026-09-19 13:42:28.257111');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (3, 1, 'UPLOAD', 'attachment', 2, NULL, '{"file_name": "rasp1 (4).xlsx", "file_size": 22793, "mime_type": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "status_id": 2, "uploaded_by": 1, "attachments_id": 2, "interaction_id": 94}', '2026-09-19 13:44:25.683435');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (4, 1, 'STATUS_CHANGE', 'interaction', 124, '{"StatusId": 1, "StatusName": "Поиск контактов ответственного в вузе"}', '{"Comment": "Нашлм контакты отвественных", "StatusId": 2, "StatusName": "Коммуникация и уточнение актуальности программ"}', '2026-09-19 13:45:19.717441');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (5, 1, 'COMMENT', 'interaction', 124, NULL, '{"Comment": "Передали материалы"}', '2026-09-19 13:45:30.847767');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (6, 1, 'STATUS_CHANGE', 'interaction', 124, '{"StatusId": 2, "StatusName": "Коммуникация и уточнение актуальности программ"}', '{"Comment": "Переход на обратный этап", "StatusId": 3, "StatusName": "Организация встречи с представителями вуза"}', '2026-09-19 13:45:44.931064');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (7, 1, 'STATUS_CHANGE', 'interaction', 124, '{"StatusId": 3, "StatusName": "Организация встречи с представителями вуза"}', '{"Comment": "а", "StatusId": 2, "StatusName": "Коммуникация и уточнение актуальности программ"}', '2026-09-19 13:45:53.690807');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (8, 1, 'STATUS_CHANGE', 'interaction', 124, '{"StatusId": 2, "StatusName": "Коммуникация и уточнение актуальности программ"}', '{"Comment": "Переход на самый первые этап", "StatusId": 1, "StatusName": "Поиск контактов ответственного в вузе"}', '2026-09-19 13:46:02.984528');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (9, 1, 'DELETE', 'attachment', 2, '{"file_name": "rasp1 (4).xlsx", "file_size": 22793, "mime_type": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "status_id": 2, "uploaded_by": 1, "storage_path": "uploads/attachments/94/2/e910ab2d78b84846804321d0d127e88b_rasp1 (4).xlsx", "attachments_id": 2, "interaction_id": 94}', NULL, '2026-09-19 14:18:00.240127');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (10, 7, 'UPDATE', 'it_direction', 4, '{"name": "Backend-разработка", "priority": null, "is_active": true, "description": "Серверная разработка на Python, Java, C#", "it_directions_id": 4}', '{"name": "Backend-разработка", "priority": 0, "is_active": true, "description": "Серверная разработка на Python, Java, C#", "it_directions_id": 4}', '2026-09-21 12:11:14.242602');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (11, 7, 'UPDATE', 'it_direction', 4, '{"name": "Backend-разработка", "priority": 0, "is_active": true, "description": "Серверная разработка на Python, Java, C#", "it_directions_id": 4}', '{"name": "Backend-разработка", "priority": 2, "is_active": true, "description": "Серверная разработка на Python, Java, C#", "it_directions_id": 4}', '2026-09-21 12:11:23.370176');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (12, 7, 'UPDATE', 'it_direction', 9, '{"name": "Веб-разработка", "priority": null, "is_active": true, "description": "Fullstack-разработка веб-приложений", "it_directions_id": 9}', '{"name": "Веб-разработка", "priority": 1, "is_active": true, "description": "Fullstack-разработка веб-приложений", "it_directions_id": 9}', '2026-09-21 12:11:26.615111');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (13, 7, 'UPDATE', 'interaction', 62, '{"manager_id": 1, "product_id": 9, "program_id": 15, "university_id": 121, "university_contact_id": null}', '{"manager_id": 3, "product_id": 9, "program_id": 15, "university_id": 121, "university_contact_id": null}', '2026-09-21 12:11:53.96263');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (14, 7, 'UPDATE', 'interaction', 107, '{"manager_id": null, "product_id": 3, "program_id": 22, "university_id": 113, "university_contact_id": null}', '{"manager_id": 4, "product_id": 3, "program_id": 22, "university_id": 113, "university_contact_id": null}', '2026-09-21 12:12:00.334602');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (15, 7, 'UPDATE', 'interaction', 23, '{"manager_id": null, "product_id": 1, "program_id": 16, "university_id": 58, "university_contact_id": null}', '{"manager_id": 4, "product_id": 1, "program_id": 16, "university_id": 58, "university_contact_id": null}', '2026-09-21 12:12:04.384513');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (16, 7, 'UPDATE', 'interaction', 24, '{"manager_id": 1, "product_id": 10, "program_id": 18, "university_id": 131, "university_contact_id": null}', '{"manager_id": 5, "product_id": 10, "program_id": 18, "university_id": 131, "university_contact_id": null}', '2026-09-21 12:12:24.252095');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (17, 7, 'UPDATE', 'interaction', 24, '{"manager_id": 5, "product_id": 10, "program_id": 18, "university_id": 131, "university_contact_id": null}', '{"manager_id": 5, "product_id": 10, "program_id": 19, "university_id": 131, "university_contact_id": null}', '2026-09-21 12:32:07.358221');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (18, 7, 'UPDATE', 'interaction', 24, '{"manager_id": 5, "product_id": 10, "program_id": 19, "university_id": 131, "university_contact_id": null}', '{"manager_id": 5, "product_id": 10, "program_id": 18, "university_id": 131, "university_contact_id": null}', '2026-09-21 12:32:14.388198');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (19, 7, 'UPDATE', 'it_direction', 4, '{"name": "Backend-разработка", "priority": 2, "is_active": true, "description": "Серверная разработка на Python, Java, C#", "it_directions_id": 4}', '{"name": "Backend-разработка", "priority": 2, "is_active": true, "description": "Серверная разработка на Python, Java, C#", "it_directions_id": 4}', '2026-09-21 12:41:21.6726');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (20, 7, 'UPDATE', 'it_direction', 10, '{"name": "Сети и телекоммуникации", "priority": null, "is_active": true, "description": "Проектирование и эксплуатация сетей передачи данных", "it_directions_id": 10}', '{"name": "Сети и телекоммуникации", "priority": 1, "is_active": true, "description": "Проектирование и эксплуатация сетей передачи данных", "it_directions_id": 10}', '2026-09-21 12:41:25.613313');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (21, 7, 'UPDATE', 'it_direction', 10, '{"name": "Сети и телекоммуникации", "priority": 1, "is_active": true, "description": "Проектирование и эксплуатация сетей передачи данных", "it_directions_id": 10}', '{"name": "Сети и телекоммуникации", "priority": 2, "is_active": true, "description": "Проектирование и эксплуатация сетей передачи данных", "it_directions_id": 10}', '2026-09-21 12:50:05.07149');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (22, 7, 'UPDATE', 'it_direction', 7, '{"name": "Информационная безопасность", "priority": null, "is_active": true, "description": "Защита информации, этичный хакинг, соответствие требованиям", "it_directions_id": 7}', '{"name": "Информационная безопасность", "priority": 1, "is_active": true, "description": "Защита информации, этичный хакинг, соответствие требованиям", "it_directions_id": 7}', '2026-09-21 12:50:10.765486');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (23, 7, 'UPDATE', 'it_direction', 8, '{"name": "Системная аналитика", "priority": null, "is_active": true, "description": "Сбор и формализация требований, моделирование процессов", "it_directions_id": 8}', '{"name": "Системная аналитика", "priority": 3, "is_active": true, "description": "Сбор и формализация требований, моделирование процессов", "it_directions_id": 8}', '2026-09-21 12:50:14.035878');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (24, 7, 'UPDATE', 'it_direction', 2, '{"name": "QA", "priority": null, "is_active": true, "description": "Тестирование программного обеспечения: ручное и автоматизированное", "it_directions_id": 2}', '{"name": "QA", "priority": 5, "is_active": true, "description": "Тестирование программного обеспечения: ручное и автоматизированное", "it_directions_id": 2}', '2026-09-21 12:50:17.790442');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (25, 7, 'UPDATE', 'it_direction', 5, '{"name": "Frontend-разработка", "priority": null, "is_active": true, "description": "Клиентская разработка: JavaScript, TypeScript, React", "it_directions_id": 5}', '{"name": "Frontend-разработка", "priority": 6, "is_active": true, "description": "Клиентская разработка: JavaScript, TypeScript, React", "it_directions_id": 5}', '2026-09-21 12:50:23.185894');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (26, 7, 'UPDATE', 'it_direction', 5, '{"name": "Frontend-разработка", "priority": 6, "is_active": true, "description": "Клиентская разработка: JavaScript, TypeScript, React", "it_directions_id": 5}', '{"name": "Frontend-разработка", "priority": 5, "is_active": true, "description": "Клиентская разработка: JavaScript, TypeScript, React", "it_directions_id": 5}', '2026-09-21 12:50:25.64351');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (27, 7, 'UPDATE', 'it_direction', 5, '{"name": "Frontend-разработка", "priority": 5, "is_active": true, "description": "Клиентская разработка: JavaScript, TypeScript, React", "it_directions_id": 5}', '{"name": "Frontend-разработка", "priority": 1, "is_active": true, "description": "Клиентская разработка: JavaScript, TypeScript, React", "it_directions_id": 5}', '2026-09-21 12:50:29.975075');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (28, 7, 'UPDATE', 'it_direction', 3, '{"name": "Data Engineering", "priority": null, "is_active": true, "description": "Построение конвейеров данных, хранилищ и озёр данных", "it_directions_id": 3}', '{"name": "Data Engineering", "priority": 2, "is_active": true, "description": "Построение конвейеров данных, хранилищ и озёр данных", "it_directions_id": 3}', '2026-09-21 12:50:32.649444');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (29, 7, 'UPDATE', 'it_direction', 3, '{"name": "Data Engineering", "priority": 2, "is_active": true, "description": "Построение конвейеров данных, хранилищ и озёр данных", "it_directions_id": 3}', '{"name": "Data Engineering", "priority": 3, "is_active": true, "description": "Построение конвейеров данных, хранилищ и озёр данных", "it_directions_id": 3}', '2026-09-21 12:50:34.730271');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (30, 7, 'UPDATE', 'it_direction', 4, '{"name": "Backend-разработка", "priority": 2, "is_active": true, "description": "Серверная разработка на Python, Java, C#", "it_directions_id": 4}', '{"name": "Backend-разработка", "priority": 3, "is_active": true, "description": "Серверная разработка на Python, Java, C#", "it_directions_id": 4}', '2026-09-21 12:50:35.435873');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (31, 7, 'UPDATE', 'it_direction', 5, '{"name": "Frontend-разработка", "priority": 1, "is_active": true, "description": "Клиентская разработка: JavaScript, TypeScript, React", "it_directions_id": 5}', '{"name": "Frontend-разработка", "priority": 1, "is_active": true, "description": "Клиентская разработка: JavaScript, TypeScript, React", "it_directions_id": 5}', '2026-09-21 12:50:38.259269');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (32, 7, 'UPDATE', 'it_direction', 3, '{"name": "Data Engineering", "priority": 3, "is_active": true, "description": "Построение конвейеров данных, хранилищ и озёр данных", "it_directions_id": 3}', '{"name": "Data Engineering", "priority": 3, "is_active": true, "description": "Построение конвейеров данных, хранилищ и озёр данных", "it_directions_id": 3}', '2026-09-21 12:50:40.84171');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (33, 7, 'UPDATE', 'it_direction', 1, '{"name": "DevOps", "priority": null, "is_active": true, "description": "Практики непрерывной интеграции и доставки, инфраструктурная автоматизация", "it_directions_id": 1}', '{"name": "DevOps", "priority": 3, "is_active": true, "description": "Практики непрерывной интеграции и доставки, инфраструктурная автоматизация", "it_directions_id": 1}', '2026-09-21 12:50:44.314173');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (34, 7, 'UPDATE', 'it_direction', 7, '{"name": "Информационная безопасность", "priority": 1, "is_active": true, "description": "Защита информации, этичный хакинг, соответствие требованиям", "it_directions_id": 7}', '{"name": "Информационная безопасность", "priority": 2, "is_active": true, "description": "Защита информации, этичный хакинг, соответствие требованиям", "it_directions_id": 7}', '2026-09-21 12:50:49.888519');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (35, 7, 'UPDATE', 'it_direction', 8, '{"name": "Системная аналитика", "priority": 3, "is_active": true, "description": "Сбор и формализация требований, моделирование процессов", "it_directions_id": 8}', '{"name": "Системная аналитика", "priority": 4, "is_active": true, "description": "Сбор и формализация требований, моделирование процессов", "it_directions_id": 8}', '2026-09-21 12:50:52.244864');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (36, 7, 'UPDATE', 'it_direction', 6, '{"name": "Машинное обучение", "priority": null, "is_active": true, "description": "ML/AI: обучение моделей, компьютерное зрение, NLP", "it_directions_id": 6}', '{"name": "Машинное обучение", "priority": 4, "is_active": true, "description": "ML/AI: обучение моделей, компьютерное зрение, NLP", "it_directions_id": 6}', '2026-09-21 12:50:57.834756');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (37, 7, 'UPDATE', 'it_direction', 1, '{"name": "DevOps", "priority": 3, "is_active": true, "description": "Практики непрерывной интеграции и доставки, инфраструктурная автоматизация", "it_directions_id": 1}', '{"name": "DevOps", "priority": 2, "is_active": true, "description": "Практики непрерывной интеграции и доставки, инфраструктурная автоматизация", "it_directions_id": 1}', '2026-09-21 12:51:02.08108');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (38, 7, 'UPDATE', 'it_direction', 6, '{"name": "Машинное обучение", "priority": 4, "is_active": true, "description": "ML/AI: обучение моделей, компьютерное зрение, NLP", "it_directions_id": 6}', '{"name": "Машинное обучение", "priority": 3, "is_active": true, "description": "ML/AI: обучение моделей, компьютерное зрение, NLP", "it_directions_id": 6}', '2026-09-21 12:51:04.035332');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (39, 7, 'UPDATE', 'it_direction', 9, '{"name": "Веб-разработка", "priority": 1, "is_active": true, "description": "Fullstack-разработка веб-приложений", "it_directions_id": 9}', '{"name": "Веб-разработка", "priority": 1, "is_active": true, "description": "Fullstack-разработка веб-приложений", "it_directions_id": 9}', '2026-09-21 16:18:19.616648');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (40, 7, 'UPDATE_USER_ACCESS', 'users', 9, '{"role": "user", "isActive": true}', '{"role": "manager", "isActive": true}', '2026-09-25 10:07:43.017585');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (41, 7, 'UPDATE_USER_ACCESS', 'users', 9, '{"role": "manager", "isActive": true}', '{"role": "user", "isActive": true}', '2026-09-25 10:07:47.795959');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (42, 7, 'UPDATE_USER_ACCESS', 'users', 9, '{"role": "user", "isActive": true}', '{"role": "user", "isActive": false}', '2026-09-25 10:07:50.285609');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (43, 7, 'UPDATE_USER_ACCESS', 'users', 9, '{"role": "user", "isActive": false}', '{"role": "user", "isActive": true}', '2026-09-25 10:08:03.463209');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (44, 7, 'UPLOAD', 'attachment', 3, NULL, '{"file_name": "Хакатон-1.zip", "file_size": 18554, "mime_type": "application/zip", "status_id": 1, "uploaded_by": 7, "attachments_id": 3, "interaction_id": 24}', '2026-09-25 15:59:17.025529');
INSERT INTO public.audit_logs (audit_logs_id, user_id, action, entity_type, entity_id, old_data, new_data, created_at) VALUES (45, 7, 'UPLOAD', 'attachment', 4, NULL, '{"file_name": "access-user.jpg", "file_size": 65095, "mime_type": "image/jpeg", "status_id": 1, "uploaded_by": 7, "attachments_id": 4, "interaction_id": 24}', '2026-09-25 18:50:45.857839');


--
-- TOC entry 5219 (class 0 OID 54585)
-- Dependencies: 230
-- Data for Name: contracts; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (1, 'ДГ-2025/001', '2026-06-27 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (2, 'ДГ-2025/002', '2026-05-08 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (3, 'ДГ-2025/003', '2026-02-14 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (4, 'ДГ-2025/004', '2026-05-19 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (5, 'ДГ-2025/005', '2026-04-29 16:32:08', 'Завершён', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (6, 'ДГ-2025/006', '2026-05-02 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (7, 'ДГ-2025/007', '2026-06-11 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (8, 'ДГ-2025/008', '2026-03-12 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (9, 'ДГ-2025/009', '2026-03-16 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (10, 'ДГ-2025/010', '2026-03-16 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (11, 'ДГ-2025/011', '2026-06-28 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (12, 'ДГ-2025/012', '2026-03-14 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (13, 'ДГ-2025/013', '2026-05-21 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (14, 'ДГ-2025/014', '2026-02-22 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (15, 'ДГ-2025/015', '2026-07-09 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (16, 'ДГ-2025/016', '2026-05-10 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (17, 'ДГ-2025/017', '2026-02-19 16:32:08', 'Завершён', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (18, 'ДГ-2025/018', '2026-01-30 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (19, 'ДГ-2025/019', '2026-05-09 16:32:08', 'Завершён', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (20, 'ДГ-2025/020', '2026-07-19 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (21, 'ДГ-2025/021', '2026-04-02 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (22, 'ДГ-2025/022', '2026-05-15 16:32:08', 'Завершён', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (23, 'ДГ-2025/023', '2026-04-19 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (24, 'ДГ-2025/024', '2026-04-19 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (25, 'ДГ-2025/025', '2026-04-10 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (26, 'ДГ-2025/026', '2026-02-26 16:32:08', 'Завершён', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (27, 'ДГ-2025/027', '2026-06-02 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (28, 'ДГ-2025/028', '2026-03-02 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (29, 'ДГ-2025/029', '2026-07-03 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (30, 'ДГ-2025/030', '2026-03-14 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (31, 'ДГ-2025/031', '2026-03-03 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (32, 'ДГ-2025/032', '2026-02-12 16:32:08', 'Завершён', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (33, 'ДГ-2025/033', '2026-02-15 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (34, 'ДГ-2025/034', '2026-06-19 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (35, 'ДГ-2025/035', '2026-04-12 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (36, 'ДГ-2025/036', '2026-04-26 16:32:08', 'Завершён', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (37, 'ДГ-2025/037', '2026-04-19 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (38, 'ДГ-2025/038', '2026-06-01 16:32:08', 'Завершён', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (39, 'ДГ-2025/039', '2026-03-13 16:32:08', 'На подписании', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.contracts (contracts_id, contract_number, signed_at, status, comment, created_at, valid_until) VALUES (40, 'ДГ-2025/040', '2026-03-15 16:32:08', 'Активен', 'Договор о сотрудничестве (демо)', '2026-09-19 13:04:41.179002', NULL);


--
-- TOC entry 5243 (class 0 OID 54847)
-- Dependencies: 254
-- Data for Name: import_batches; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5237 (class 0 OID 54774)
-- Dependencies: 248
-- Data for Name: interaction_status_history; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (1, 1, NULL, 1, NULL, NULL, '2026-04-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (2, 2, NULL, 1, NULL, NULL, '2026-06-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (3, 2, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-05-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (4, 2, 2, 3, NULL, 'Уточнили актуальность программ по направлению', '2026-05-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (5, 2, 3, 4, NULL, 'Проведено обучение преподавателей', '2026-04-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (6, 2, 4, 5, NULL, 'Провели встречу с представителями вуза', '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (7, 2, 5, 6, NULL, 'Проведено обучение преподавателей', '2026-05-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (8, 2, 6, 7, NULL, 'Передали пакет документов на согласование', '2026-04-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (9, 2, 7, 8, NULL, 'Проведено обучение преподавателей', '2026-04-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (10, 2, 8, 9, NULL, 'Уточнили актуальность программ по направлению', '2026-04-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (11, 2, 9, 11, NULL, 'Программа актуализирована с учётом продукта', '2026-04-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (12, 3, NULL, 1, NULL, NULL, '2026-06-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (13, 3, 1, 2, NULL, 'Внесены правки в документы по замечаниям', '2026-06-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (14, 3, 2, 4, NULL, 'Переданы лицензии и обучающие материалы', '2026-05-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (15, 3, 4, 5, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (16, 4, NULL, 1, NULL, NULL, '2026-09-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (17, 5, NULL, 1, NULL, NULL, '2026-01-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (18, 6, NULL, 1, NULL, NULL, '2026-09-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (19, 6, 1, 2, NULL, 'Переданы лицензии и обучающие материалы', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (20, 6, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (21, 6, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-09-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (22, 6, 4, 5, NULL, 'Внесены правки в документы по замечаниям', '2026-08-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (23, 6, 5, 6, NULL, 'Проведено обучение преподавателей', '2026-09-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (24, 6, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-09-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (25, 6, 7, 8, NULL, 'Провели встречу с представителями вуза', '2026-09-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (26, 6, 8, 9, NULL, 'Начато сопровождение внедрения', '2026-08-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (27, 6, 9, 10, NULL, 'Передали пакет документов на согласование', '2026-08-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (28, 6, 10, 11, NULL, 'Провели встречу с представителями вуза', '2026-08-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (29, 6, 11, 12, NULL, 'Связались с ответственным, договорились о встрече', '2026-08-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (30, 6, 12, 13, NULL, 'Программа актуализирована с учётом продукта', '2026-08-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (31, 7, NULL, 2, NULL, NULL, '2026-09-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (32, 8, NULL, 1, NULL, NULL, '2026-03-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (33, 8, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-03-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (34, 9, NULL, 2, NULL, NULL, '2026-07-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (35, 9, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-07-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (36, 10, NULL, 1, NULL, NULL, '2026-06-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (37, 10, 1, 2, NULL, 'Начато сопровождение внедрения', '2026-06-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (38, 11, NULL, 1, NULL, NULL, '2026-04-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (39, 11, 1, 2, NULL, 'Начато сопровождение внедрения', '2026-05-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (40, 11, 2, 3, NULL, 'Провели встречу с представителями вуза', '2026-04-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (41, 11, 3, 4, NULL, 'Передали пакет документов на согласование', '2026-04-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (42, 11, 4, 5, NULL, 'Связались с ответственным, договорились о встрече', '2026-04-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (43, 11, 5, 6, NULL, 'Переданы лицензии и обучающие материалы', '2026-04-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (44, 11, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-04-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (45, 12, NULL, 2, NULL, NULL, '2026-07-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (46, 13, NULL, 1, NULL, NULL, '2026-05-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (47, 14, NULL, 2, NULL, NULL, '2026-09-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (48, 14, 2, 4, NULL, 'Внесены правки в документы по замечаниям', '2026-09-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (49, 14, 4, 5, NULL, 'Провели встречу с представителями вуза', '2026-09-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (50, 14, 5, 6, NULL, 'Уточнили актуальность программ по направлению', '2026-09-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (51, 15, NULL, 2, NULL, NULL, '2026-05-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (52, 15, 2, 3, NULL, 'Документы подписаны', '2026-05-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (53, 15, 3, 4, NULL, 'Начато сопровождение внедрения', '2026-05-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (54, 15, 4, 5, NULL, 'Уточнили актуальность программ по направлению', '2026-05-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (55, 15, 5, 6, NULL, 'Документы подписаны', '2026-05-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (56, 15, 6, 7, NULL, 'Документы подписаны', '2026-04-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (57, 15, 7, 8, NULL, 'Переданы лицензии и обучающие материалы', '2026-04-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (58, 15, 8, 9, NULL, 'Программа актуализирована с учётом продукта', '2026-04-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (59, 16, NULL, 1, NULL, NULL, '2026-07-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (60, 17, NULL, 1, NULL, NULL, '2026-08-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (61, 17, 1, 2, NULL, 'Начато сопровождение внедрения', '2026-07-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (62, 17, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-07-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (63, 18, NULL, 1, NULL, NULL, '2026-03-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (64, 19, NULL, 1, NULL, NULL, '2026-09-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (65, 19, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (66, 19, 2, 3, NULL, 'Проведено обучение преподавателей', '2026-09-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (67, 19, 3, 4, NULL, 'Документы подписаны', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (68, 19, 4, 5, NULL, 'Программа актуализирована с учётом продукта', '2026-09-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (69, 20, NULL, 1, NULL, NULL, '2026-04-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (70, 20, 1, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-04-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (71, 21, NULL, 1, NULL, NULL, '2026-07-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (72, 21, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-07-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (73, 21, 2, 3, NULL, 'Внесены правки в документы по замечаниям', '2026-08-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (74, 21, 3, 4, NULL, 'Внесены правки в документы по замечаниям', '2026-07-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (75, 21, 4, 5, NULL, 'Уточнили актуальность программ по направлению', '2026-07-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (76, 21, 5, 7, NULL, 'Переданы лицензии и обучающие материалы', '2026-07-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (77, 21, 7, 8, NULL, 'Программа актуализирована с учётом продукта', '2026-07-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (78, 21, 8, 9, NULL, 'Провели встречу с представителями вуза', '2026-07-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (79, 21, 9, 10, NULL, 'Уточнили актуальность программ по направлению', '2026-07-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (80, 22, NULL, 1, NULL, NULL, '2026-06-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (81, 23, NULL, 1, NULL, NULL, '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (82, 23, 1, 3, NULL, 'Провели встречу с представителями вуза', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (83, 23, 3, 4, NULL, 'Начато сопровождение внедрения', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (84, 23, 4, 5, NULL, 'Связались с ответственным, договорились о встрече', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (85, 23, 5, 7, NULL, 'Внесены правки в документы по замечаниям', '2026-09-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (86, 24, NULL, 1, NULL, NULL, '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (87, 24, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (88, 24, 2, 4, NULL, 'Переданы лицензии и обучающие материалы', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (89, 24, 4, 5, NULL, 'Документы подписаны', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (90, 24, 5, 6, NULL, 'Передали пакет документов на согласование', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (91, 24, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-09-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (92, 24, 7, 8, NULL, 'Связались с ответственным, договорились о встрече', '2026-09-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (93, 25, NULL, 1, NULL, NULL, '2026-02-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (94, 25, 1, 2, NULL, 'Начато сопровождение внедрения', '2026-02-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (95, 26, NULL, 1, NULL, NULL, '2026-05-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (96, 26, 1, 2, NULL, 'Внесены правки в документы по замечаниям', '2026-05-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (97, 27, NULL, 1, NULL, NULL, '2026-07-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (98, 28, NULL, 2, NULL, NULL, '2026-08-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (99, 29, NULL, 1, NULL, NULL, '2026-08-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (100, 30, NULL, 1, NULL, NULL, '2026-05-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (101, 30, 1, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-05-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (102, 30, 3, 4, NULL, 'Переданы лицензии и обучающие материалы', '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (103, 30, 4, 6, NULL, 'Внесены правки в документы по замечаниям', '2026-05-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (104, 31, NULL, 1, NULL, NULL, '2026-09-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (105, 31, 1, 3, NULL, 'Переданы лицензии и обучающие материалы', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (106, 31, 3, 4, NULL, 'Переданы лицензии и обучающие материалы', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (107, 31, 4, 6, NULL, 'Начато сопровождение внедрения', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (108, 31, 6, 7, NULL, 'Документы подписаны', '2026-09-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (109, 31, 7, 8, NULL, 'Провели встречу с представителями вуза', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (110, 31, 8, 9, NULL, 'Программа актуализирована с учётом продукта', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (111, 31, 9, 10, NULL, 'Провели встречу с представителями вуза', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (112, 31, 10, 11, NULL, 'Уточнили актуальность программ по направлению', '2026-09-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (113, 31, 11, 12, NULL, 'Внесены правки в документы по замечаниям', '2026-09-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (114, 31, 12, 13, NULL, 'Переданы лицензии и обучающие материалы', '2026-08-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (115, 31, 13, 14, NULL, 'Программа актуализирована с учётом продукта', '2026-09-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (116, 32, NULL, 1, NULL, NULL, '2026-07-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (117, 32, 1, 2, NULL, 'Передали пакет документов на согласование', '2026-07-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (118, 32, 2, 3, NULL, 'Документы подписаны', '2026-07-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (119, 32, 3, 4, NULL, 'Начато сопровождение внедрения', '2026-06-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (120, 33, NULL, 1, NULL, NULL, '2026-06-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (121, 33, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-06-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (122, 34, NULL, 2, NULL, NULL, '2026-03-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (123, 34, 2, 3, NULL, 'Начато сопровождение внедрения', '2026-03-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (124, 34, 3, 4, NULL, 'Внесены правки в документы по замечаниям', '2026-03-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (125, 34, 4, 5, NULL, 'Внесены правки в документы по замечаниям', '2026-03-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (126, 34, 5, 6, NULL, 'Переданы лицензии и обучающие материалы', '2026-03-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (127, 34, 6, 8, NULL, 'Уточнили актуальность программ по направлению', '2026-03-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (128, 35, NULL, 1, NULL, NULL, '2026-05-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (129, 35, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-05-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (130, 35, 2, 3, NULL, 'Провели встречу с представителями вуза', '2026-04-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (131, 35, 3, 4, NULL, 'Передали пакет документов на согласование', '2026-04-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (132, 36, NULL, 1, NULL, NULL, '2026-05-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (133, 37, NULL, 1, NULL, NULL, '2026-08-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (134, 38, NULL, 2, NULL, NULL, '2026-04-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (135, 38, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-04-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (136, 38, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-04-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (137, 38, 4, 6, NULL, 'Уточнили актуальность программ по направлению', '2026-03-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (138, 39, NULL, 1, NULL, NULL, '2026-08-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (139, 40, NULL, 1, NULL, NULL, '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (140, 40, 1, 2, NULL, 'Документы подписаны', '2026-05-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (141, 41, NULL, 1, NULL, NULL, '2026-02-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (142, 42, NULL, 1, NULL, NULL, '2026-03-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (143, 42, 1, 2, NULL, 'Передали пакет документов на согласование', '2026-03-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (144, 42, 2, 3, NULL, 'Переданы лицензии и обучающие материалы', '2026-03-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (145, 42, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-03-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (146, 42, 4, 5, NULL, 'Провели встречу с представителями вуза', '2026-03-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (147, 42, 5, 6, NULL, 'Программа актуализирована с учётом продукта', '2026-03-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (148, 42, 6, 7, NULL, 'Переданы лицензии и обучающие материалы', '2026-03-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (149, 42, 7, 8, NULL, 'Провели встречу с представителями вуза', '2026-02-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (150, 43, NULL, 1, NULL, NULL, '2026-05-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (594, 151, NULL, 1, NULL, NULL, '2026-07-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (151, 43, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-05-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (152, 43, 2, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-05-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (153, 43, 3, 4, NULL, 'Документы подписаны', '2026-04-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (154, 43, 4, 5, NULL, 'Начато сопровождение внедрения', '2026-04-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (155, 43, 5, 6, NULL, 'Передали пакет документов на согласование', '2026-04-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (156, 43, 6, 7, NULL, 'Программа актуализирована с учётом продукта', '2026-04-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (157, 43, 7, 8, NULL, 'Уточнили актуальность программ по направлению', '2026-04-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (158, 43, 8, 9, NULL, 'Документы подписаны', '2026-03-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (159, 43, 9, 10, NULL, 'Провели встречу с представителями вуза', '2026-03-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (160, 43, 10, 11, NULL, 'Уточнили актуальность программ по направлению', '2026-03-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (161, 43, 11, 12, NULL, 'Документы подписаны', '2026-03-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (162, 44, NULL, 3, NULL, NULL, '2026-06-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (163, 45, NULL, 1, NULL, NULL, '2026-04-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (164, 45, 1, 2, NULL, 'Начато сопровождение внедрения', '2026-04-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (165, 45, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-04-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (166, 45, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-04-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (167, 45, 4, 5, NULL, 'Передали пакет документов на согласование', '2026-04-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (168, 46, NULL, 2, NULL, NULL, '2026-06-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (169, 46, 2, 3, NULL, 'Уточнили актуальность программ по направлению', '2026-05-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (170, 46, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (171, 46, 4, 5, NULL, 'Внесены правки в документы по замечаниям', '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (172, 46, 5, 6, NULL, 'Внесены правки в документы по замечаниям', '2026-05-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (173, 47, NULL, 1, NULL, NULL, '2026-08-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (174, 47, 1, 2, NULL, 'Внесены правки в документы по замечаниям', '2026-08-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (175, 47, 2, 3, NULL, 'Документы подписаны', '2026-08-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (176, 48, NULL, 1, NULL, NULL, '2026-03-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (177, 48, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-04-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (178, 48, 2, 3, NULL, 'Документы подписаны', '2026-05-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (179, 48, 3, 4, NULL, 'Переданы лицензии и обучающие материалы', '2026-05-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (180, 48, 4, 5, NULL, 'Программа актуализирована с учётом продукта', '2026-04-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (181, 48, 5, 6, NULL, 'Провели встречу с представителями вуза', '2026-04-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (182, 48, 6, 7, NULL, 'Связались с ответственным, договорились о встрече', '2026-03-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (183, 48, 7, 8, NULL, 'Документы подписаны', '2026-04-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (184, 48, 8, 9, NULL, 'Внесены правки в документы по замечаниям', '2026-04-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (185, 48, 9, 10, NULL, 'Проведено обучение преподавателей', '2026-04-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (186, 48, 10, 11, NULL, 'Документы подписаны', '2026-03-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (187, 48, 11, 12, NULL, 'Внесены правки в документы по замечаниям', '2026-03-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (188, 49, NULL, 1, NULL, NULL, '2026-02-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (189, 49, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-02-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (190, 49, 2, 4, NULL, 'Документы подписаны', '2026-02-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (191, 50, NULL, 1, NULL, NULL, '2026-08-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (192, 50, 1, 2, NULL, 'Проведено обучение преподавателей', '2026-08-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (193, 50, 2, 3, NULL, 'Документы подписаны', '2026-08-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (194, 50, 3, 4, NULL, 'Начато сопровождение внедрения', '2026-08-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (195, 50, 4, 5, NULL, 'Программа актуализирована с учётом продукта', '2026-08-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (196, 51, NULL, 1, NULL, NULL, '2026-06-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (197, 51, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-07-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (198, 51, 2, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-06-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (199, 51, 3, 4, NULL, 'Документы подписаны', '2026-06-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (200, 51, 4, 5, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (201, 51, 5, 6, NULL, 'Начато сопровождение внедрения', '2026-05-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (202, 51, 6, 7, NULL, 'Уточнили актуальность программ по направлению', '2026-05-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (203, 51, 7, 8, NULL, 'Начато сопровождение внедрения', '2026-05-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (204, 51, 8, 9, NULL, 'Связались с ответственным, договорились о встрече', '2026-06-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (205, 51, 9, 10, NULL, 'Провели встречу с представителями вуза', '2026-05-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (206, 52, NULL, 2, NULL, NULL, '2026-02-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (207, 53, NULL, 1, NULL, NULL, '2026-02-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (208, 53, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-02-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (209, 53, 2, 3, NULL, 'Начато сопровождение внедрения', '2026-02-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (210, 53, 3, 4, NULL, 'Передали пакет документов на согласование', '2026-02-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (211, 53, 4, 6, NULL, 'Провели встречу с представителями вуза', '2026-02-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (212, 53, 6, 7, NULL, 'Начато сопровождение внедрения', '2026-02-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (213, 54, NULL, 1, NULL, NULL, '2026-07-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (214, 54, 1, 2, NULL, 'Документы подписаны', '2026-07-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (215, 54, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-07-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (216, 54, 3, 4, NULL, 'Передали пакет документов на согласование', '2026-07-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (217, 54, 4, 5, NULL, 'Уточнили актуальность программ по направлению', '2026-07-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (218, 54, 5, 7, NULL, 'Программа актуализирована с учётом продукта', '2026-07-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (219, 54, 7, 8, NULL, 'Программа актуализирована с учётом продукта', '2026-06-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (220, 54, 8, 9, NULL, 'Начато сопровождение внедрения', '2026-06-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (221, 55, NULL, 1, NULL, NULL, '2026-03-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (222, 55, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-03-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (223, 55, 2, 3, NULL, 'Документы подписаны', '2026-03-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (224, 55, 3, 4, NULL, 'Документы подписаны', '2026-03-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (225, 55, 4, 5, NULL, 'Провели встречу с представителями вуза', '2026-03-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (226, 55, 5, 6, NULL, 'Уточнили актуальность программ по направлению', '2026-03-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (227, 56, NULL, 1, NULL, NULL, '2026-05-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (228, 56, 1, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (229, 56, 3, 4, NULL, 'Начато сопровождение внедрения', '2026-04-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (230, 56, 4, 5, NULL, 'Уточнили актуальность программ по направлению', '2026-04-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (231, 56, 5, 6, NULL, 'Начато сопровождение внедрения', '2026-04-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (232, 57, NULL, 1, NULL, NULL, '2026-05-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (233, 58, NULL, 1, NULL, NULL, '2026-06-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (234, 58, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-06-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (235, 58, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-06-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (236, 58, 3, 4, NULL, 'Проведено обучение преподавателей', '2026-06-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (237, 58, 4, 5, NULL, 'Проведено обучение преподавателей', '2026-06-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (238, 58, 5, 6, NULL, 'Переданы лицензии и обучающие материалы', '2026-06-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (239, 59, NULL, 1, NULL, NULL, '2026-06-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (240, 59, 1, 2, NULL, 'Переданы лицензии и обучающие материалы', '2026-05-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (241, 59, 2, 3, NULL, 'Уточнили актуальность программ по направлению', '2026-05-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (242, 59, 3, 4, NULL, 'Переданы лицензии и обучающие материалы', '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (243, 59, 4, 5, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (244, 59, 5, 6, NULL, 'Уточнили актуальность программ по направлению', '2026-05-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (245, 59, 6, 7, NULL, 'Уточнили актуальность программ по направлению', '2026-05-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (246, 60, NULL, 1, NULL, NULL, '2026-09-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (247, 60, 1, 2, NULL, 'Документы подписаны', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (248, 60, 2, 3, NULL, 'Проведено обучение преподавателей', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (249, 60, 3, 4, NULL, 'Передали пакет документов на согласование', '2026-09-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (250, 60, 4, 5, NULL, 'Передали пакет документов на согласование', '2026-09-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (251, 60, 5, 6, NULL, 'Уточнили актуальность программ по направлению', '2026-09-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (252, 61, NULL, 3, NULL, NULL, '2026-07-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (253, 61, 3, 4, NULL, 'Начато сопровождение внедрения', '2026-07-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (254, 62, NULL, 1, NULL, NULL, '2026-09-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (255, 62, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-09-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (256, 62, 2, 3, NULL, 'Провели встречу с представителями вуза', '2026-09-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (257, 63, NULL, 1, NULL, NULL, '2026-02-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (258, 64, NULL, 1, NULL, NULL, '2026-02-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (259, 65, NULL, 1, NULL, NULL, '2026-07-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (260, 65, 1, 2, NULL, 'Документы подписаны', '2026-07-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (261, 66, NULL, 1, NULL, NULL, '2026-02-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (262, 66, 1, 4, NULL, 'Внесены правки в документы по замечаниям', '2026-02-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (263, 66, 4, 5, NULL, 'Уточнили актуальность программ по направлению', '2026-02-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (264, 66, 5, 6, NULL, 'Начато сопровождение внедрения', '2026-02-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (265, 66, 6, 7, NULL, 'Документы подписаны', '2026-02-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (266, 66, 7, 8, NULL, 'Внесены правки в документы по замечаниям', '2026-02-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (267, 67, NULL, 1, NULL, NULL, '2026-04-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (268, 67, 1, 2, NULL, 'Проведено обучение преподавателей', '2026-05-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (269, 67, 2, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (270, 67, 4, 5, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (271, 67, 5, 6, NULL, 'Внесены правки в документы по замечаниям', '2026-05-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (272, 67, 6, 7, NULL, 'Провели встречу с представителями вуза', '2026-04-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (273, 67, 7, 8, NULL, 'Провели встречу с представителями вуза', '2026-05-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (274, 67, 8, 9, NULL, 'Начато сопровождение внедрения', '2026-04-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (275, 67, 9, 10, NULL, 'Передали пакет документов на согласование', '2026-04-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (276, 67, 10, 11, NULL, 'Уточнили актуальность программ по направлению', '2026-04-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (277, 68, NULL, 1, NULL, NULL, '2026-06-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (278, 68, 1, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-06-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (279, 68, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-06-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (280, 68, 4, 5, NULL, 'Передали пакет документов на согласование', '2026-06-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (281, 69, NULL, 1, NULL, NULL, '2026-02-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (282, 69, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-02-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (283, 70, NULL, 1, NULL, NULL, '2026-03-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (284, 71, NULL, 1, NULL, NULL, '2026-05-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (285, 71, 1, 2, NULL, 'Внесены правки в документы по замечаниям', '2026-05-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (286, 71, 2, 3, NULL, 'Внесены правки в документы по замечаниям', '2026-05-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (287, 72, NULL, 1, NULL, NULL, '2026-02-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (288, 73, NULL, 1, NULL, NULL, '2026-07-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (289, 73, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-08-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (290, 73, 2, 3, NULL, 'Уточнили актуальность программ по направлению', '2026-08-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (291, 73, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-07-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (292, 73, 4, 5, NULL, 'Передали пакет документов на согласование', '2026-07-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (293, 73, 5, 6, NULL, 'Переданы лицензии и обучающие материалы', '2026-07-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (294, 73, 6, 8, NULL, 'Уточнили актуальность программ по направлению', '2026-07-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (295, 73, 8, 9, NULL, 'Программа актуализирована с учётом продукта', '2026-07-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (296, 73, 9, 10, NULL, 'Уточнили актуальность программ по направлению', '2026-07-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (297, 74, NULL, 1, NULL, NULL, '2026-02-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (298, 74, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-02-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (299, 75, NULL, 1, NULL, NULL, '2026-01-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (300, 75, 1, 2, NULL, 'Переданы лицензии и обучающие материалы', '2026-01-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (301, 76, NULL, 1, NULL, NULL, '2026-02-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (302, 76, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-01-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (303, 77, NULL, 1, NULL, NULL, '2026-04-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (304, 77, 1, 2, NULL, 'Внесены правки в документы по замечаниям', '2026-04-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (305, 78, NULL, 1, NULL, NULL, '2026-08-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (306, 78, 1, 2, NULL, 'Проведено обучение преподавателей', '2026-08-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (307, 78, 2, 3, NULL, 'Документы подписаны', '2026-08-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (308, 78, 3, 4, NULL, 'Проведено обучение преподавателей', '2026-08-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (309, 79, NULL, 1, NULL, NULL, '2026-08-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (310, 79, 1, 2, NULL, 'Начато сопровождение внедрения', '2026-08-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (311, 80, NULL, 1, NULL, NULL, '2026-05-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (312, 80, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-05-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (313, 80, 2, 4, NULL, 'Документы подписаны', '2026-06-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (314, 80, 4, 5, NULL, 'Программа актуализирована с учётом продукта', '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (315, 80, 5, 6, NULL, 'Документы подписаны', '2026-04-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (316, 80, 6, 7, NULL, 'Программа актуализирована с учётом продукта', '2026-05-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (317, 80, 7, 9, NULL, 'Программа актуализирована с учётом продукта', '2026-05-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (318, 80, 9, 10, NULL, 'Начато сопровождение внедрения', '2026-05-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (319, 80, 10, 11, NULL, 'Внесены правки в документы по замечаниям', '2026-05-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (320, 80, 11, 12, NULL, 'Начато сопровождение внедрения', '2026-04-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (321, 80, 12, 13, NULL, 'Уточнили актуальность программ по направлению', '2026-04-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (322, 81, NULL, 1, NULL, NULL, '2026-08-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (323, 81, 1, 2, NULL, 'Документы подписаны', '2026-08-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (324, 81, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-07-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (325, 81, 3, 4, NULL, 'Программа актуализирована с учётом продукта', '2026-07-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (326, 81, 4, 5, NULL, 'Уточнили актуальность программ по направлению', '2026-07-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (327, 81, 5, 6, NULL, 'Документы подписаны', '2026-07-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (328, 82, NULL, 1, NULL, NULL, '2026-04-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (329, 82, 1, 2, NULL, 'Проведено обучение преподавателей', '2026-04-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (330, 82, 2, 3, NULL, 'Документы подписаны', '2026-05-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (331, 82, 3, 4, NULL, 'Программа актуализирована с учётом продукта', '2026-04-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (332, 82, 4, 5, NULL, 'Переданы лицензии и обучающие материалы', '2026-04-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (333, 82, 5, 6, NULL, 'Внесены правки в документы по замечаниям', '2026-04-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (334, 82, 6, 7, NULL, 'Передали пакет документов на согласование', '2026-04-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (335, 82, 7, 8, NULL, 'Программа актуализирована с учётом продукта', '2026-04-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (336, 82, 8, 9, NULL, 'Программа актуализирована с учётом продукта', '2026-04-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (337, 83, NULL, 1, NULL, NULL, '2026-07-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (338, 83, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-07-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (339, 83, 2, 5, NULL, 'Документы подписаны', '2026-07-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (340, 83, 5, 6, NULL, 'Передали пакет документов на согласование', '2026-07-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (341, 84, NULL, 1, NULL, NULL, '2026-04-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (342, 84, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-04-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (343, 84, 2, 3, NULL, 'Переданы лицензии и обучающие материалы', '2026-04-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (344, 84, 3, 4, NULL, 'Переданы лицензии и обучающие материалы', '2026-04-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (345, 84, 4, 5, NULL, 'Переданы лицензии и обучающие материалы', '2026-04-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (346, 84, 5, 6, NULL, 'Переданы лицензии и обучающие материалы', '2026-04-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (347, 84, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-04-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (348, 84, 7, 8, NULL, 'Уточнили актуальность программ по направлению', '2026-04-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (349, 84, 8, 9, NULL, 'Программа актуализирована с учётом продукта', '2026-04-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (350, 85, NULL, 2, NULL, NULL, '2026-05-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (351, 86, NULL, 1, NULL, NULL, '2026-09-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (352, 87, NULL, 1, NULL, NULL, '2026-02-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (353, 87, 1, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-02-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (354, 87, 3, 4, NULL, 'Передали пакет документов на согласование', '2026-02-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (355, 87, 4, 5, NULL, 'Уточнили актуальность программ по направлению', '2026-02-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (356, 87, 5, 6, NULL, 'Провели встречу с представителями вуза', '2026-02-05 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (357, 88, NULL, 1, NULL, NULL, '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (358, 88, 1, 2, NULL, 'Документы подписаны', '2026-05-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (359, 88, 2, 3, NULL, 'Переданы лицензии и обучающие материалы', '2026-05-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (360, 88, 3, 4, NULL, 'Внесены правки в документы по замечаниям', '2026-05-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (361, 88, 4, 5, NULL, 'Начато сопровождение внедрения', '2026-05-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (362, 88, 5, 6, NULL, 'Уточнили актуальность программ по направлению', '2026-05-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (363, 88, 6, 7, NULL, 'Провели встречу с представителями вуза', '2026-05-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (364, 88, 7, 9, NULL, 'Внесены правки в документы по замечаниям', '2026-05-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (365, 89, NULL, 1, NULL, NULL, '2026-07-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (366, 90, NULL, 1, NULL, NULL, '2026-04-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (367, 91, NULL, 2, NULL, NULL, '2026-08-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (368, 92, NULL, 1, NULL, NULL, '2026-02-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (369, 93, NULL, 1, NULL, NULL, '2026-06-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (370, 93, 1, 3, NULL, 'Начато сопровождение внедрения', '2026-06-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (371, 93, 3, 4, NULL, 'Провели встречу с представителями вуза', '2026-06-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (372, 93, 4, 5, NULL, 'Внесены правки в документы по замечаниям', '2026-06-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (373, 94, NULL, 1, NULL, NULL, '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (374, 94, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (375, 94, 2, 3, NULL, 'Провели встречу с представителями вуза', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (376, 94, 3, 4, NULL, 'Программа актуализирована с учётом продукта', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (377, 94, 4, 5, NULL, 'Документы подписаны', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (378, 94, 5, 6, NULL, 'Уточнили актуальность программ по направлению', '2026-09-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (379, 94, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (380, 94, 7, 8, NULL, 'Проведено обучение преподавателей', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (381, 94, 8, 9, NULL, 'Внесены правки в документы по замечаниям', '2026-09-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (382, 94, 9, 10, NULL, 'Начато сопровождение внедрения', '2026-09-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (383, 95, NULL, 2, NULL, NULL, '2026-05-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (384, 95, 2, 3, NULL, 'Уточнили актуальность программ по направлению', '2026-05-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (385, 95, 3, 4, NULL, 'Программа актуализирована с учётом продукта', '2026-05-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (386, 95, 4, 5, NULL, 'Внесены правки в документы по замечаниям', '2026-05-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (387, 96, NULL, 1, NULL, NULL, '2026-07-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (388, 96, 1, 2, NULL, 'Переданы лицензии и обучающие материалы', '2026-07-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (389, 96, 2, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-07-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (390, 96, 3, 4, NULL, 'Переданы лицензии и обучающие материалы', '2026-07-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (391, 97, NULL, 3, NULL, NULL, '2026-03-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (392, 98, NULL, 1, NULL, NULL, '2026-06-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (393, 99, NULL, 1, NULL, NULL, '2026-08-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (394, 99, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-08-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (395, 100, NULL, 1, NULL, NULL, '2026-02-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (396, 100, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-01-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (397, 101, NULL, 1, NULL, NULL, '2026-08-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (398, 101, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-08-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (399, 101, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-08-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (400, 101, 3, 4, NULL, 'Провели встречу с представителями вуза', '2026-08-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (401, 102, NULL, 1, NULL, NULL, '2026-05-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (402, 102, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-05-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (403, 102, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-05-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (404, 102, 3, 4, NULL, 'Программа актуализирована с учётом продукта', '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (405, 103, NULL, 1, NULL, NULL, '2026-08-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (406, 103, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-08-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (407, 103, 2, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-08-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (408, 104, NULL, 1, NULL, NULL, '2026-08-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (409, 104, 1, 3, NULL, 'Передали пакет документов на согласование', '2026-08-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (410, 104, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-08-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (411, 104, 4, 5, NULL, 'Внесены правки в документы по замечаниям', '2026-07-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (412, 104, 5, 6, NULL, 'Внесены правки в документы по замечаниям', '2026-08-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (413, 104, 6, 7, NULL, 'Переданы лицензии и обучающие материалы', '2026-07-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (414, 105, NULL, 2, NULL, NULL, '2026-08-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (415, 105, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-07-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (416, 105, 3, 4, NULL, 'Документы подписаны', '2026-07-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (417, 106, NULL, 1, NULL, NULL, '2026-07-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (418, 106, 1, 2, NULL, 'Внесены правки в документы по замечаниям', '2026-07-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (419, 106, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-07-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (420, 106, 3, 4, NULL, 'Провели встречу с представителями вуза', '2026-07-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (421, 106, 4, 5, NULL, 'Программа актуализирована с учётом продукта', '2026-07-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (422, 106, 5, 6, NULL, 'Документы подписаны', '2026-07-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (423, 107, NULL, 1, NULL, NULL, '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (424, 107, 1, 2, NULL, 'Документы подписаны', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (425, 107, 2, 4, NULL, 'Внесены правки в документы по замечаниям', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (426, 107, 4, 5, NULL, 'Проведено обучение преподавателей', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (427, 107, 5, 6, NULL, 'Внесены правки в документы по замечаниям', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (428, 107, 6, 7, NULL, 'Начато сопровождение внедрения', '2026-09-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (429, 107, 7, 8, NULL, 'Внесены правки в документы по замечаниям', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (430, 107, 8, 9, NULL, 'Передали пакет документов на согласование', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (431, 107, 9, 10, NULL, 'Уточнили актуальность программ по направлению', '2026-09-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (432, 108, NULL, 1, NULL, NULL, '2026-03-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (433, 108, 1, 2, NULL, 'Внесены правки в документы по замечаниям', '2026-03-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (434, 109, NULL, 1, NULL, NULL, '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (435, 109, 1, 2, NULL, 'Внесены правки в документы по замечаниям', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (436, 109, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-09-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (437, 109, 3, 4, NULL, 'Передали пакет документов на согласование', '2026-09-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (438, 110, NULL, 3, NULL, NULL, '2026-05-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (439, 110, 3, 4, NULL, 'Начато сопровождение внедрения', '2026-04-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (440, 111, NULL, 2, NULL, NULL, '2026-04-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (441, 112, NULL, 1, NULL, NULL, '2026-03-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (442, 112, 1, 2, NULL, 'Начато сопровождение внедрения', '2026-03-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (443, 112, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-03-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (444, 112, 3, 4, NULL, 'Проведено обучение преподавателей', '2026-03-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (445, 112, 4, 5, NULL, 'Документы подписаны', '2026-03-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (446, 113, NULL, 1, NULL, NULL, '2026-03-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (447, 113, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-04-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (448, 113, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-03-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (449, 113, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-02-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (450, 113, 4, 6, NULL, 'Внесены правки в документы по замечаниям', '2026-03-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (451, 113, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-03-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (452, 113, 7, 9, NULL, 'Проведено обучение преподавателей', '2026-03-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (453, 113, 9, 10, NULL, 'Переданы лицензии и обучающие материалы', '2026-02-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (454, 113, 10, 11, NULL, 'Документы подписаны', '2026-02-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (455, 113, 11, 12, NULL, 'Документы подписаны', '2026-02-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (456, 114, NULL, 1, NULL, NULL, '2026-09-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (457, 114, 1, 2, NULL, 'Проведено обучение преподавателей', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (458, 115, NULL, 2, NULL, NULL, '2026-03-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (459, 116, NULL, 1, NULL, NULL, '2026-04-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (460, 117, NULL, 2, NULL, NULL, '2026-06-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (461, 117, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (462, 118, NULL, 1, NULL, NULL, '2026-09-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (463, 118, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-09-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (464, 118, 2, 3, NULL, 'Провели встречу с представителями вуза', '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (465, 118, 3, 4, NULL, 'Проведено обучение преподавателей', '2026-09-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (466, 118, 4, 5, NULL, 'Документы подписаны', '2026-08-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (467, 118, 5, 6, NULL, 'Внесены правки в документы по замечаниям', '2026-09-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (468, 118, 6, 7, NULL, 'Связались с ответственным, договорились о встрече', '2026-08-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (469, 119, NULL, 1, NULL, NULL, '2026-03-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (470, 119, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-03-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (471, 120, NULL, 1, NULL, NULL, '2026-08-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (472, 121, NULL, 1, NULL, NULL, '2026-07-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (473, 121, 1, 3, NULL, 'Документы подписаны', '2026-07-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (474, 121, 3, 4, NULL, 'Переданы лицензии и обучающие материалы', '2026-07-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (475, 122, NULL, 1, NULL, NULL, '2026-03-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (476, 122, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-02-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (477, 122, 2, 3, NULL, 'Переданы лицензии и обучающие материалы', '2026-03-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (478, 122, 3, 4, NULL, 'Внесены правки в документы по замечаниям', '2026-03-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (479, 122, 4, 5, NULL, 'Начато сопровождение внедрения', '2026-02-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (480, 122, 5, 6, NULL, 'Передали пакет документов на согласование', '2026-02-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (481, 122, 6, 9, NULL, 'Передали пакет документов на согласование', '2026-02-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (482, 122, 9, 10, NULL, 'Передали пакет документов на согласование', '2026-02-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (483, 122, 10, 11, NULL, 'Переданы лицензии и обучающие материалы', '2026-02-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (484, 122, 11, 12, NULL, 'Начато сопровождение внедрения', '2026-02-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (485, 123, NULL, 1, NULL, NULL, '2026-04-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (486, 123, 1, 2, NULL, 'Уточнили актуальность программ по направлению', '2026-04-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (487, 124, NULL, 1, NULL, NULL, '2026-09-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (488, 125, NULL, 1, NULL, NULL, '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (489, 125, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (490, 125, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-03-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (491, 125, 3, 4, NULL, 'Начато сопровождение внедрения', '2026-05-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (492, 125, 4, 5, NULL, 'Передали пакет документов на согласование', '2026-04-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (493, 125, 5, 6, NULL, 'Начато сопровождение внедрения', '2026-03-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (494, 125, 6, 7, NULL, 'Провели встречу с представителями вуза', '2026-04-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (495, 125, 7, 8, NULL, 'Внесены правки в документы по замечаниям', '2026-03-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (496, 125, 8, 9, NULL, 'Проведено обучение преподавателей', '2026-03-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (497, 125, 9, 10, NULL, 'Проведено обучение преподавателей', '2026-03-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (498, 125, 10, 11, NULL, 'Переданы лицензии и обучающие материалы', '2026-03-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (499, 125, 11, 12, NULL, 'Уточнили актуальность программ по направлению', '2026-03-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (500, 125, 12, 13, NULL, 'Начато сопровождение внедрения', '2026-03-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (501, 126, NULL, 1, NULL, NULL, '2026-02-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (502, 127, NULL, 1, NULL, NULL, '2026-05-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (503, 127, 1, 3, NULL, 'Начато сопровождение внедрения', '2026-04-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (504, 127, 3, 4, NULL, 'Документы подписаны', '2026-04-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (505, 127, 4, 5, NULL, 'Провели встречу с представителями вуза', '2026-04-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (506, 127, 5, 7, NULL, 'Документы подписаны', '2026-04-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (507, 127, 7, 8, NULL, 'Программа актуализирована с учётом продукта', '2026-04-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (508, 127, 8, 9, NULL, 'Начато сопровождение внедрения', '2026-04-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (509, 127, 9, 10, NULL, 'Начато сопровождение внедрения', '2026-04-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (510, 127, 10, 11, NULL, 'Уточнили актуальность программ по направлению', '2026-04-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (511, 128, NULL, 1, NULL, NULL, '2026-06-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (512, 129, NULL, 1, NULL, NULL, '2026-09-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (513, 129, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-09-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (514, 129, 2, 3, NULL, 'Переданы лицензии и обучающие материалы', '2026-08-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (515, 129, 3, 4, NULL, 'Начато сопровождение внедрения', '2026-08-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (516, 129, 4, 6, NULL, 'Документы подписаны', '2026-08-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (517, 129, 6, 7, NULL, 'Переданы лицензии и обучающие материалы', '2026-08-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (518, 129, 7, 8, NULL, 'Программа актуализирована с учётом продукта', '2026-08-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (519, 130, NULL, 1, NULL, NULL, '2026-04-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (520, 130, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-04-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (521, 130, 2, 3, NULL, 'Проведено обучение преподавателей', '2026-04-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (522, 130, 3, 4, NULL, 'Программа актуализирована с учётом продукта', '2026-04-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (523, 131, NULL, 1, NULL, NULL, '2026-06-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (524, 132, NULL, 1, NULL, NULL, '2026-02-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (525, 132, 1, 2, NULL, 'Программа актуализирована с учётом продукта', '2026-02-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (526, 132, 2, 3, NULL, 'Внесены правки в документы по замечаниям', '2026-02-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (527, 132, 3, 4, NULL, 'Документы подписаны', '2026-02-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (528, 132, 4, 5, NULL, 'Провели встречу с представителями вуза', '2026-02-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (529, 133, NULL, 1, NULL, NULL, '2026-04-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (530, 133, 1, 2, NULL, 'Начато сопровождение внедрения', '2026-04-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (531, 133, 2, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-04-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (532, 133, 3, 4, NULL, 'Проведено обучение преподавателей', '2026-04-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (533, 133, 4, 5, NULL, 'Переданы лицензии и обучающие материалы', '2026-03-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (534, 133, 5, 6, NULL, 'Проведено обучение преподавателей', '2026-03-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (535, 133, 6, 7, NULL, 'Связались с ответственным, договорились о встрече', '2026-03-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (536, 134, NULL, 1, NULL, NULL, '2026-04-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (537, 135, NULL, 1, NULL, NULL, '2026-07-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (538, 135, 1, 2, NULL, 'Проведено обучение преподавателей', '2026-08-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (539, 135, 2, 3, NULL, 'Проведено обучение преподавателей', '2026-07-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (540, 135, 3, 5, NULL, 'Провели встречу с представителями вуза', '2026-07-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (541, 135, 5, 7, NULL, 'Внесены правки в документы по замечаниям', '2026-07-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (542, 135, 7, 8, NULL, 'Переданы лицензии и обучающие материалы', '2026-07-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (543, 136, NULL, 1, NULL, NULL, '2026-07-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (544, 136, 1, 2, NULL, 'Проведено обучение преподавателей', '2026-07-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (545, 136, 2, 3, NULL, 'Провели встречу с представителями вуза', '2026-07-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (546, 136, 3, 4, NULL, 'Внесены правки в документы по замечаниям', '2026-07-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (547, 136, 4, 6, NULL, 'Передали пакет документов на согласование', '2026-07-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (548, 136, 6, 8, NULL, 'Передали пакет документов на согласование', '2026-07-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (549, 137, NULL, 1, NULL, NULL, '2026-06-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (550, 138, NULL, 1, NULL, NULL, '2026-08-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (551, 139, NULL, 1, NULL, NULL, '2026-02-27 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (552, 139, 1, 2, NULL, 'Документы подписаны', '2026-02-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (553, 139, 2, 3, NULL, 'Документы подписаны', '2026-01-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (554, 139, 3, 5, NULL, 'Провели встречу с представителями вуза', '2026-02-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (555, 139, 5, 6, NULL, 'Проведено обучение преподавателей', '2026-01-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (556, 139, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-01-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (557, 139, 7, 9, NULL, 'Переданы лицензии и обучающие материалы', '2026-01-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (558, 140, NULL, 2, NULL, NULL, '2026-09-02 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (559, 140, 2, 3, NULL, 'Документы подписаны', '2026-08-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (560, 141, NULL, 1, NULL, NULL, '2026-05-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (561, 141, 1, 2, NULL, 'Переданы лицензии и обучающие материалы', '2026-04-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (562, 141, 2, 4, NULL, 'Проведено обучение преподавателей', '2026-05-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (563, 141, 4, 5, NULL, 'Связались с ответственным, договорились о встрече', '2026-05-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (564, 141, 5, 6, NULL, 'Документы подписаны', '2026-05-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (565, 141, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-05-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (566, 141, 7, 8, NULL, 'Начато сопровождение внедрения', '2026-05-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (567, 141, 8, 9, NULL, 'Внесены правки в документы по замечаниям', '2026-04-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (568, 141, 9, 10, NULL, 'Документы подписаны', '2026-04-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (569, 141, 10, 11, NULL, 'Уточнили актуальность программ по направлению', '2026-04-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (570, 141, 11, 12, NULL, 'Передали пакет документов на согласование', '2026-04-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (571, 142, NULL, 1, NULL, NULL, '2026-04-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (572, 143, NULL, 1, NULL, NULL, '2026-07-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (573, 144, NULL, 1, NULL, NULL, '2026-05-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (574, 145, NULL, 1, NULL, NULL, '2026-03-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (575, 146, NULL, 1, NULL, NULL, '2026-04-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (576, 146, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-04-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (577, 146, 2, 3, NULL, 'Уточнили актуальность программ по направлению', '2026-04-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (578, 147, NULL, 1, NULL, NULL, '2026-03-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (579, 147, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-03-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (580, 147, 2, 3, NULL, 'Передали пакет документов на согласование', '2026-03-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (581, 147, 3, 5, NULL, 'Провели встречу с представителями вуза', '2026-03-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (582, 147, 5, 6, NULL, 'Переданы лицензии и обучающие материалы', '2026-02-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (583, 147, 6, 7, NULL, 'Внесены правки в документы по замечаниям', '2026-03-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (584, 147, 7, 8, NULL, 'Внесены правки в документы по замечаниям', '2026-02-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (585, 147, 8, 9, NULL, 'Связались с ответственным, договорились о встрече', '2026-02-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (586, 148, NULL, 1, NULL, NULL, '2026-08-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (587, 149, NULL, 1, NULL, NULL, '2026-05-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (588, 149, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-05-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (589, 149, 2, 3, NULL, 'Внесены правки в документы по замечаниям', '2026-05-17 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (590, 149, 3, 4, NULL, 'Уточнили актуальность программ по направлению', '2026-05-06 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (591, 149, 4, 6, NULL, 'Программа актуализирована с учётом продукта', '2026-05-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (592, 150, NULL, 3, NULL, NULL, '2026-04-09 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (593, 150, 3, 4, NULL, 'Уточнили актуальность программ по направлению', '2026-04-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (595, 151, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-06-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (596, 151, 2, 5, NULL, 'Начато сопровождение внедрения', '2026-06-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (597, 152, NULL, 1, NULL, NULL, '2026-04-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (598, 152, 1, 2, NULL, 'Провели встречу с представителями вуза', '2026-04-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (599, 152, 2, 3, NULL, 'Переданы лицензии и обучающие материалы', '2026-04-14 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (600, 152, 3, 4, NULL, 'Документы подписаны', '2026-04-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (601, 152, 4, 5, NULL, 'Документы подписаны', '2026-04-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (602, 152, 5, 6, NULL, 'Начато сопровождение внедрения', '2026-04-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (603, 152, 6, 7, NULL, 'Документы подписаны', '2026-04-08 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (604, 153, NULL, 1, NULL, NULL, '2026-08-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (605, 153, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-08-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (606, 153, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-09-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (607, 153, 3, 4, NULL, 'Программа актуализирована с учётом продукта', '2026-09-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (608, 153, 4, 5, NULL, 'Программа актуализирована с учётом продукта', '2026-08-31 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (609, 153, 5, 6, NULL, 'Связались с ответственным, договорились о встрече', '2026-08-25 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (610, 154, NULL, 1, NULL, NULL, '2026-02-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (611, 154, 1, 2, NULL, 'Передали пакет документов на согласование', '2026-02-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (612, 154, 2, 3, NULL, 'Проведено обучение преподавателей', '2026-02-15 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (613, 154, 3, 5, NULL, 'Уточнили актуальность программ по направлению', '2026-02-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (614, 154, 5, 6, NULL, 'Документы подписаны', '2026-02-07 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (615, 155, NULL, 1, NULL, NULL, '2026-08-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (616, 155, 1, 2, NULL, 'Связались с ответственным, договорились о встрече', '2026-08-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (617, 155, 2, 3, NULL, 'Начато сопровождение внедрения', '2026-08-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (618, 155, 3, 4, NULL, 'Уточнили актуальность программ по направлению', '2026-07-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (619, 155, 4, 5, NULL, 'Документы подписаны', '2026-08-01 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (620, 155, 5, 6, NULL, 'Связались с ответственным, договорились о встрече', '2026-07-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (621, 155, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-07-24 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (622, 155, 7, 8, NULL, 'Переданы лицензии и обучающие материалы', '2026-07-23 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (623, 156, NULL, 1, NULL, NULL, '2026-09-10 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (624, 157, NULL, 1, NULL, NULL, '2026-08-16 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (625, 157, 1, 2, NULL, 'Начато сопровождение внедрения', '2026-08-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (626, 157, 2, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-08-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (627, 157, 3, 4, NULL, 'Проведено обучение преподавателей', '2026-07-22 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (628, 157, 4, 5, NULL, 'Начато сопровождение внедрения', '2026-07-28 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (629, 157, 5, 6, NULL, 'Провели встречу с представителями вуза', '2026-07-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (630, 157, 6, 7, NULL, 'Проведено обучение преподавателей', '2026-07-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (631, 157, 7, 8, NULL, 'Провели встречу с представителями вуза', '2026-07-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (632, 157, 8, 9, NULL, 'Передали пакет документов на согласование', '2026-07-20 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (633, 157, 9, 10, NULL, 'Документы подписаны', '2026-07-18 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (634, 157, 10, 11, NULL, 'Уточнили актуальность программ по направлению', '2026-07-19 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (635, 158, NULL, 1, NULL, NULL, '2026-03-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (636, 158, 1, 2, NULL, 'Передали пакет документов на согласование', '2026-03-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (637, 158, 2, 3, NULL, 'Связались с ответственным, договорились о встрече', '2026-03-04 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (638, 158, 3, 4, NULL, 'Связались с ответственным, договорились о встрече', '2026-02-26 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (639, 159, NULL, 1, NULL, NULL, '2026-08-21 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (640, 159, 1, 2, NULL, 'Переданы лицензии и обучающие материалы', '2026-09-03 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (641, 159, 2, 3, NULL, 'Программа актуализирована с учётом продукта', '2026-08-29 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (642, 159, 3, 4, NULL, 'Внесены правки в документы по замечаниям', '2026-08-12 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (643, 159, 4, 5, NULL, 'Провели встречу с представителями вуза', '2026-08-11 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (644, 159, 5, 6, NULL, 'Программа актуализирована с учётом продукта', '2026-08-13 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (645, 160, NULL, 1, NULL, NULL, '2026-07-30 16:32:08');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (646, 94, 10, 11, 1, 'Актуализировали учебную программу', '2026-09-19 13:42:01.598579');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (647, 124, 1, 2, 1, 'Нашлм контакты отвественных', '2026-09-19 13:45:19.688495');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (648, 124, 2, 2, 1, 'Передали материалы', '2026-09-19 13:45:30.843403');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (649, 124, 2, 3, 1, 'Переход на обратный этап', '2026-09-19 13:45:44.925159');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (650, 124, 3, 2, 1, 'а', '2026-09-19 13:45:53.683076');
INSERT INTO public.interaction_status_history (interaction_status_history_id, interaction_id, from_status_id, to_status_id, changed_by, comment, changed_at) VALUES (651, 124, 2, 1, 1, 'Переход на самый первые этап', '2026-09-19 13:46:02.975151');


--
-- TOC entry 5235 (class 0 OID 54720)
-- Dependencies: 246
-- Data for Name: interactions; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (1, 115, 21, 5, NULL, NULL, 1, 3, NULL, NULL, '2026-04-15 16:32:08', '2026-04-22 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (8, 103, 22, 1, 1, NULL, 1, 2, NULL, NULL, '2026-03-14 16:32:08', '2026-03-15 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (10, 27, 22, 9, 1, NULL, 1, 2, NULL, NULL, '2026-06-06 16:32:08', '2026-06-15 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (16, 130, 8, 3, 1, NULL, 1, 1, NULL, NULL, '2026-07-15 16:32:08', '2026-08-01 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (5, 67, 18, 1, NULL, NULL, 1, 1, NULL, NULL, '2026-01-23 16:32:08', '2026-02-07 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (22, 111, 21, 1, 1, NULL, 1, 1, NULL, NULL, '2026-06-08 16:32:08', '2026-06-20 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (7, 3, 1, 4, NULL, NULL, 1, 2, NULL, NULL, '2026-09-02 16:32:08', '2026-09-05 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (26, 49, 16, 4, 1, NULL, 1, 3, NULL, NULL, '2026-05-01 16:32:08', '2026-05-13 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (32, 40, 13, 6, 1, NULL, 1, 4, NULL, NULL, '2026-06-26 16:32:08', '2026-07-09 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (44, 85, 5, 9, 1, NULL, 1, 3, NULL, NULL, '2026-06-04 16:32:08', '2026-06-23 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (50, 22, 8, 7, 1, NULL, 1, 5, NULL, NULL, '2026-08-14 16:32:08', '2026-09-01 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (52, 128, 5, 5, 1, NULL, 1, 2, NULL, NULL, '2026-02-08 16:32:08', '2026-02-16 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (17, 10, 5, 4, NULL, NULL, 1, 3, NULL, NULL, '2026-07-23 16:32:08', '2026-08-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (70, 137, 3, 4, 1, NULL, 1, 1, NULL, NULL, '2026-03-04 16:32:08', '2026-03-22 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (19, 116, 18, 1, NULL, NULL, 1, 5, NULL, NULL, '2026-09-11 16:32:08', '2026-09-13 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (74, 51, 1, 2, 1, NULL, 1, 2, NULL, NULL, '2026-01-31 16:32:08', '2026-02-20 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (86, 100, 11, 5, 1, NULL, 1, 1, NULL, NULL, '2026-09-01 16:32:08', '2026-09-11 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (92, 142, 15, 5, 1, NULL, 1, 2, NULL, NULL, '2026-02-17 16:32:08', '2026-02-25 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (25, 109, 4, 7, NULL, NULL, 1, 2, NULL, NULL, '2026-02-17 16:32:08', '2026-02-19 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (98, 124, 9, 6, 1, NULL, 1, 1, NULL, NULL, '2026-06-25 16:32:08', '2026-07-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (100, 88, 7, 8, 1, NULL, 1, 2, NULL, NULL, '2026-01-29 16:32:08', '2026-02-11 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (29, 91, 12, 5, NULL, NULL, 1, 1, NULL, NULL, '2026-08-17 16:32:08', '2026-08-19 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (78, 23, 13, 3, 1, NULL, 1, 4, NULL, 1, '2026-07-30 16:32:08', '2026-08-19 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (62, 121, 15, 9, 3, NULL, 1, 3, NULL, NULL, '2026-09-05 16:32:08', '2026-09-21 12:11:53.928069');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (107, 113, 22, 3, 4, NULL, 1, 10, NULL, NULL, '2026-09-13 16:32:08', '2026-09-21 12:12:00.32742');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (23, 58, 16, 1, 4, NULL, 1, 7, NULL, NULL, '2026-09-17 16:32:08', '2026-09-21 12:12:04.368206');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (35, 56, 15, 10, NULL, NULL, 1, 4, NULL, NULL, '2026-04-23 16:32:08', '2026-05-05 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (37, 133, 14, 2, NULL, NULL, 1, 1, NULL, NULL, '2026-08-25 16:32:08', '2026-09-02 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (41, 19, 3, 5, NULL, NULL, 1, 1, NULL, NULL, '2026-02-24 16:32:08', '2026-02-27 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (43, 97, 7, 5, NULL, NULL, 1, 12, NULL, NULL, '2026-03-23 16:32:08', '2026-03-25 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (47, 73, 8, 9, NULL, NULL, 1, 3, NULL, NULL, '2026-08-07 16:32:08', '2026-08-21 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (49, 136, 19, 5, NULL, NULL, 1, 5, NULL, NULL, '2026-02-01 16:32:08', '2026-02-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (53, 101, 3, 6, NULL, NULL, 1, 8, NULL, NULL, '2026-01-31 16:32:08', '2026-02-20 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (55, 81, 12, 4, NULL, NULL, 1, 7, NULL, NULL, '2026-03-12 16:32:08', '2026-03-16 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (59, 57, 13, 2, NULL, NULL, 1, 7, NULL, NULL, '2026-05-05 16:32:08', '2026-05-22 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (61, 34, 1, 6, NULL, NULL, 1, 4, NULL, NULL, '2026-07-12 16:32:08', '2026-07-28 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (65, 112, 21, 7, NULL, NULL, 1, 2, NULL, NULL, '2026-06-29 16:32:08', '2026-07-06 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (71, 138, 4, 3, NULL, NULL, 1, 4, NULL, NULL, '2026-05-23 16:32:08', '2026-06-02 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (77, 54, 20, 10, NULL, NULL, 1, 2, NULL, NULL, '2026-04-07 16:32:08', '2026-04-16 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (79, 5, 11, 7, NULL, NULL, 1, 2, NULL, NULL, '2026-08-04 16:32:08', '2026-08-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (83, 12, 2, 3, NULL, NULL, 1, 6, NULL, NULL, '2026-06-30 16:32:08', '2026-07-19 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (85, 83, 21, 4, NULL, NULL, 1, 2, NULL, NULL, '2026-05-27 16:32:08', '2026-05-29 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (89, 26, 3, 8, NULL, NULL, 1, 1, NULL, NULL, '2026-07-03 16:32:08', '2026-07-15 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (91, 62, 12, 7, NULL, NULL, 1, 3, NULL, NULL, '2026-08-03 16:32:08', '2026-08-17 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (95, 134, 1, 10, NULL, NULL, 1, 6, NULL, NULL, '2026-05-09 16:32:08', '2026-05-22 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (101, 127, 10, 9, NULL, NULL, 1, 4, NULL, NULL, '2026-08-16 16:32:08', '2026-09-01 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (110, 29, 7, 6, 1, NULL, 1, 4, NULL, NULL, '2026-04-27 16:32:08', '2026-05-11 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (109, 82, 15, 2, NULL, NULL, 1, 4, NULL, NULL, '2026-09-06 16:32:08', '2026-09-19 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (116, 76, 18, 10, 1, NULL, 1, 1, NULL, NULL, '2026-04-20 16:32:08', '2026-04-21 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (130, 123, 4, 8, 1, NULL, 1, 4, NULL, NULL, '2026-04-04 16:32:08', '2026-04-22 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (113, 7, 1, 6, NULL, NULL, 1, 12, NULL, NULL, '2026-02-18 16:32:08', '2026-03-06 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (134, 21, 13, 3, 1, NULL, 1, 1, NULL, NULL, '2026-04-01 16:32:08', '2026-04-02 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (115, 31, 6, 2, NULL, NULL, 1, 3, NULL, NULL, '2026-03-21 16:32:08', '2026-04-10 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (142, 140, 18, 9, 1, NULL, 1, 1, NULL, NULL, '2026-04-12 16:32:08', '2026-04-14 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (160, 10, 16, 8, 1, NULL, 1, 2, NULL, NULL, '2026-07-25 16:32:08', '2026-08-07 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (133, 139, 7, 4, NULL, NULL, 1, 7, NULL, NULL, '2026-03-21 16:32:08', '2026-03-30 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (139, 89, 11, 8, NULL, NULL, 1, 9, NULL, NULL, '2026-01-23 16:32:08', '2026-01-27 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (143, 32, 22, 7, NULL, NULL, 1, 1, NULL, NULL, '2026-07-29 16:32:08', '2026-08-15 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (149, 41, 20, 7, NULL, NULL, 1, 6, NULL, NULL, '2026-05-02 16:32:08', '2026-05-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (157, 77, 17, 5, NULL, NULL, 1, 11, NULL, NULL, '2026-07-14 16:32:08', '2026-08-01 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (4, 107, 7, 10, 1, 174, 1, 1, NULL, NULL, '2026-09-08 16:32:08', '2026-09-19 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (20, 15, 21, 10, 1, 20, 1, 4, NULL, NULL, '2026-04-04 16:32:08', '2026-04-19 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (28, 70, 13, 6, 1, 113, 1, 3, NULL, NULL, '2026-08-23 16:32:08', '2026-08-25 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (40, 65, 11, 8, 1, 105, 1, 2, NULL, NULL, '2026-05-09 16:32:08', '2026-05-24 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (11, 48, 2, 6, NULL, 76, 1, 7, NULL, NULL, '2026-04-01 16:32:08', '2026-04-13 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (13, 110, 15, 6, NULL, 178, 1, 1, NULL, NULL, '2026-05-05 16:32:08', '2026-05-11 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (31, 46, 14, 7, NULL, 71, 1, 14, NULL, NULL, '2026-08-29 16:32:08', '2026-09-15 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (67, 119, 19, 5, NULL, 192, 1, 11, NULL, NULL, '2026-04-13 16:32:08', '2026-04-20 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (73, 64, 5, 2, NULL, 103, 1, 10, NULL, NULL, '2026-07-18 16:32:08', '2026-07-29 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (97, 1, 2, 7, NULL, 2, 1, 3, NULL, NULL, '2026-03-27 16:32:08', '2026-04-03 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (103, 92, 18, 6, NULL, 148, 1, 3, NULL, NULL, '2026-08-09 16:32:08', '2026-08-17 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (119, 14, 18, 8, NULL, 19, 1, 2, NULL, NULL, '2026-03-27 16:32:08', '2026-04-11 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (121, 125, 9, 4, NULL, 201, 1, 4, NULL, NULL, '2026-07-06 16:32:08', '2026-07-14 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (125, 35, 22, 9, NULL, 50, 1, 13, NULL, NULL, '2026-03-13 16:32:08', '2026-03-31 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (127, 106, 4, 8, NULL, 172, 1, 11, NULL, NULL, '2026-04-05 16:32:08', '2026-04-05 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (131, 66, 6, 3, NULL, 106, 1, 1, NULL, NULL, '2026-06-12 16:32:08', '2026-07-01 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (137, 75, 6, 9, NULL, 122, 1, 1, NULL, NULL, '2026-06-15 16:32:08', '2026-06-16 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (145, 45, 3, 1, NULL, 70, 1, 2, NULL, NULL, '2026-02-28 16:32:08', '2026-03-08 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (151, 103, 7, 10, NULL, 166, 1, 5, NULL, NULL, '2026-06-27 16:32:08', '2026-07-08 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (155, 96, 17, 1, NULL, 155, 1, 8, NULL, NULL, '2026-07-20 16:32:08', '2026-08-08 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (64, 59, 16, 6, 1, 94, 1, 3, NULL, NULL, '2026-01-27 16:32:08', '2026-01-31 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (68, 39, 2, 4, 1, 58, 1, 5, NULL, NULL, '2026-06-06 16:32:08', '2026-06-17 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (76, 78, 6, 6, 1, 126, 1, 2, NULL, NULL, '2026-01-30 16:32:08', '2026-02-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (128, 52, 22, 4, 1, 83, 1, 1, NULL, NULL, '2026-06-24 16:32:08', '2026-07-05 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (140, 102, 18, 9, 1, 165, 1, 4, NULL, NULL, '2026-08-27 16:32:08', '2026-09-07 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (146, 20, 14, 7, 1, 26, 1, 4, NULL, NULL, '2026-04-11 16:32:08', '2026-04-23 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (148, 67, 18, 9, 1, 108, 1, 1, NULL, NULL, '2026-08-19 16:32:08', '2026-08-20 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (158, 36, 22, 4, 1, 53, 1, 4, NULL, NULL, '2026-02-24 16:32:08', '2026-03-06 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (46, 63, 7, 7, 1, 101, 1, 6, 7, NULL, '2026-05-13 16:32:08', '2026-06-02 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (38, 44, 17, 9, 1, 68, 1, 6, 12, NULL, '2026-03-27 16:32:08', '2026-04-14 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (154, 48, 18, 5, 1, NULL, 1, 6, 11, NULL, '2026-02-03 16:32:08', '2026-02-20 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (112, 86, 7, 3, 1, NULL, 1, 6, 8, NULL, '2026-03-08 16:32:08', '2026-03-13 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (106, 13, 18, 10, 1, NULL, 1, 6, 2, NULL, '2026-07-09 16:32:08', '2026-07-18 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (58, 9, 5, 7, 1, NULL, 1, 6, 6, NULL, '2026-06-04 16:32:08', '2026-06-15 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (56, 33, 8, 9, 1, NULL, 1, 6, 4, NULL, '2026-04-19 16:32:08', '2026-05-08 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (14, 77, 13, 10, 1, NULL, 1, 6, 1, NULL, '2026-09-09 16:32:08', '2026-09-11 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (152, 80, 15, 6, 1, NULL, 1, 7, 9, NULL, '2026-04-04 16:32:08', '2026-04-13 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (94, 105, 3, 7, 1, NULL, 1, 11, 3, NULL, '2026-09-11 16:32:08', '2026-09-19 13:42:01.585256');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (104, 43, 7, 4, 1, NULL, 1, 7, 13, NULL, '2026-07-24 16:32:08', '2026-07-30 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (136, 11, 13, 7, 1, 16, 1, 8, 6, NULL, '2026-07-01 16:32:08', '2026-07-18 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (124, 120, 14, 8, 1, 194, 1, 1, NULL, NULL, '2026-09-03 16:32:08', '2026-09-19 13:46:02.974341');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (118, 79, 10, 7, 1, 128, 1, 8, 1, NULL, '2026-08-25 16:32:08', '2026-09-10 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (34, 117, 18, 3, 1, 189, 1, 8, 8, NULL, '2026-03-07 16:32:08', '2026-03-24 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (88, 37, 7, 7, 1, NULL, 1, 9, 10, NULL, '2026-05-01 16:32:08', '2026-05-18 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (82, 135, 3, 6, 1, NULL, 1, 9, 4, NULL, '2026-04-03 16:32:08', '2026-04-15 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (2, 45, 5, 8, 1, NULL, 1, 11, 2, NULL, '2026-04-17 16:32:08', '2026-04-28 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (122, 90, 4, 2, 1, NULL, 1, 12, 5, NULL, '2026-02-04 16:32:08', '2026-02-05 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (3, 20, 16, 2, 6, NULL, 1, 5, NULL, 3, '2026-05-28 16:32:08', '2026-05-29 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (15, 36, 16, 4, 6, NULL, 1, 9, NULL, 4, '2026-04-18 16:32:08', '2026-04-24 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (21, 25, 1, 9, 6, NULL, 1, 10, NULL, 10, '2026-07-11 16:32:08', '2026-07-28 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (39, 94, 10, 7, 6, NULL, 1, 1, NULL, 6, '2026-08-21 16:32:08', '2026-08-25 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (45, 17, 14, 3, 6, NULL, 1, 5, NULL, 1, '2026-04-10 16:32:08', '2026-04-27 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (51, 122, 1, 2, 6, NULL, 1, 10, NULL, 7, '2026-05-25 16:32:08', '2026-06-14 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (63, 87, 17, 1, 6, NULL, 1, 1, NULL, 8, '2026-02-12 16:32:08', '2026-02-12 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (69, 2, 8, 5, 6, NULL, 1, 2, NULL, 3, '2026-02-14 16:32:08', '2026-02-24 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (87, 42, 6, 2, 6, NULL, 1, 6, NULL, 10, '2026-02-02 16:32:08', '2026-02-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (80, 126, 17, 6, 1, NULL, 1, 13, 2, NULL, '2026-04-23 16:32:08', '2026-04-30 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (12, 96, 17, 3, 1, NULL, 1, 3, NULL, 1, '2026-07-19 16:32:08', '2026-07-27 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (18, 60, 12, 3, 1, NULL, 1, 1, NULL, 7, '2026-03-19 16:32:08', '2026-04-05 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (36, 28, 21, 10, 1, NULL, 1, 1, NULL, 3, '2026-04-28 16:32:08', '2026-05-18 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (93, 84, 3, 6, 6, NULL, 1, 6, NULL, 5, '2026-06-06 16:32:08', '2026-06-24 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (99, 99, 16, 2, 6, NULL, 1, 2, NULL, 11, '2026-08-16 16:32:08', '2026-08-25 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (111, 68, 11, 3, 6, NULL, 1, 3, NULL, 1, '2026-04-10 16:32:08', '2026-04-11 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (117, 95, 2, 2, 6, NULL, 1, 4, NULL, 7, '2026-05-29 16:32:08', '2026-06-10 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (123, 72, 19, 8, 6, NULL, 1, 2, NULL, 2, '2026-04-16 16:32:08', '2026-04-28 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (129, 55, 17, 6, 6, NULL, 1, 9, NULL, 8, '2026-08-23 16:32:08', '2026-08-24 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (141, 98, 10, 4, 6, NULL, 1, 12, NULL, 9, '2026-04-14 16:32:08', '2026-04-30 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (147, 107, 19, 1, 6, NULL, 1, 9, NULL, 4, '2026-02-14 16:32:08', '2026-02-16 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (9, 80, 5, 9, 6, 129, 1, 3, NULL, 9, '2026-07-22 16:32:08', '2026-08-09 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (27, 16, 8, 2, 6, 22, 1, 1, NULL, 5, '2026-07-13 16:32:08', '2026-07-17 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (33, 6, 19, 4, 6, 8, 1, 3, NULL, 11, '2026-06-15 16:32:08', '2026-06-23 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (57, 129, 21, 10, 6, 207, 1, 1, NULL, 2, '2026-05-07 16:32:08', '2026-05-07 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (75, 118, 15, 1, 6, 190, 1, 2, NULL, 9, '2026-01-26 16:32:08', '2026-02-13 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (81, 108, 2, 4, 6, 176, 1, 6, NULL, 4, '2026-07-25 16:32:08', '2026-07-25 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (105, 53, 13, 6, 6, 85, 1, 4, NULL, 6, '2026-07-17 16:32:08', '2026-07-23 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (135, 74, 6, 5, 6, 120, 1, 8, NULL, 3, '2026-07-17 16:32:08', '2026-07-21 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (153, 27, 10, 8, 6, 38, 1, 6, NULL, 10, '2026-08-23 16:32:08', '2026-08-28 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (159, 130, 5, 10, 6, 209, 1, 6, NULL, 5, '2026-08-09 16:32:08', '2026-08-13 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (96, 71, 18, 6, 1, NULL, 1, 4, NULL, 8, '2026-07-14 16:32:08', '2026-07-26 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (114, 143, 7, 3, 1, NULL, 1, 4, NULL, 4, '2026-09-15 16:32:08', '2026-09-19 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (120, 18, 19, 8, 1, NULL, 1, 1, NULL, 10, '2026-08-27 16:32:08', '2026-09-12 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (138, 38, 18, 5, 1, NULL, 1, 1, NULL, 6, '2026-08-09 16:32:08', '2026-08-27 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (144, 115, 22, 3, 1, NULL, 1, 1, NULL, 1, '2026-05-25 16:32:08', '2026-06-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (150, 3, 2, 9, 1, NULL, 1, 4, NULL, 7, '2026-04-07 16:32:08', '2026-04-24 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (156, 110, 16, 8, 1, NULL, 1, 1, NULL, 2, '2026-09-09 16:32:08', '2026-09-19 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (72, 24, 4, 2, 1, 32, 1, 2, NULL, 6, '2026-02-25 16:32:08', '2026-03-06 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (90, 93, 22, 5, 1, 150, 1, 1, NULL, 2, '2026-04-08 16:32:08', '2026-04-25 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (102, 61, 9, 6, 1, 97, 1, 4, NULL, 3, '2026-05-13 16:32:08', '2026-05-25 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (108, 30, 8, 6, 1, 43, 1, 2, NULL, 9, '2026-03-13 16:32:08', '2026-03-22 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (126, 69, 19, 8, 1, 111, 1, 1, NULL, 5, '2026-02-12 16:32:08', '2026-02-27 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (30, 4, 15, 9, 1, 5, 1, 6, 4, 8, '2026-05-11 16:32:08', '2026-05-24 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (132, 114, 13, 7, 1, NULL, 1, 6, 2, 11, '2026-01-28 16:32:08', '2026-02-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (60, 132, 15, 10, 1, NULL, 1, 6, 8, 5, '2026-09-05 16:32:08', '2026-09-14 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (66, 104, 4, 9, 1, NULL, 1, 8, 1, 11, '2026-02-01 16:32:08', '2026-02-02 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (42, 50, 21, 2, 1, NULL, 1, 8, 3, 9, '2026-02-27 16:32:08', '2026-03-04 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (84, 8, 21, 2, 1, NULL, 1, 9, 6, 7, '2026-04-01 16:32:08', '2026-04-21 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (54, 141, 1, 1, 1, NULL, 1, 9, 2, 10, '2026-06-24 16:32:08', '2026-06-27 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (48, 47, 18, 1, 1, 73, 1, 12, 9, 4, '2026-03-19 16:32:08', '2026-04-02 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (6, 41, 14, 3, 1, NULL, 1, 13, 6, 6, '2026-08-16 16:32:08', '2026-09-05 16:32:08');
INSERT INTO public.interactions (interactions_id, university_id, program_id, product_id, manager_id, university_contact_id, workflow_id, current_status_id, contract_id, license_id, created_at, updated_at) VALUES (24, 131, 18, 10, 5, NULL, 1, 8, 11, 2, '2026-09-11 16:32:08', '2026-09-21 12:32:14.383195');


--
-- TOC entry 5209 (class 0 OID 54515)
-- Dependencies: 220
-- Data for Name: it_directions; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (10, 'Сети и телекоммуникации', 'Проектирование и эксплуатация сетей передачи данных', true, '2026-09-19 13:04:41.179002', '2026-09-21 12:50:05.067312', 2);
INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (2, 'QA', 'Тестирование программного обеспечения: ручное и автоматизированное', true, '2026-09-19 13:04:41.179002', '2026-09-21 12:50:17.786887', 5);
INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (4, 'Backend-разработка', 'Серверная разработка на Python, Java, C#', true, '2026-09-19 13:04:41.179002', '2026-09-21 12:50:35.424769', 3);
INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (5, 'Frontend-разработка', 'Клиентская разработка: JavaScript, TypeScript, React', true, '2026-09-19 13:04:41.179002', '2026-09-21 12:50:38.248161', 1);
INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (3, 'Data Engineering', 'Построение конвейеров данных, хранилищ и озёр данных', true, '2026-09-19 13:04:41.179002', '2026-09-21 12:50:40.838462', 3);
INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (7, 'Информационная безопасность', 'Защита информации, этичный хакинг, соответствие требованиям', true, '2026-09-19 13:04:41.179002', '2026-09-21 12:50:49.8867', 2);
INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (8, 'Системная аналитика', 'Сбор и формализация требований, моделирование процессов', true, '2026-09-19 13:04:41.179002', '2026-09-21 12:50:52.243166', 4);
INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (1, 'DevOps', 'Практики непрерывной интеграции и доставки, инфраструктурная автоматизация', true, '2026-09-19 13:04:41.179002', '2026-09-21 12:51:02.079363', 2);
INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (6, 'Машинное обучение', 'ML/AI: обучение моделей, компьютерное зрение, NLP', true, '2026-09-19 13:04:41.179002', '2026-09-21 12:51:04.032013', 3);
INSERT INTO public.it_directions (it_directions_id, name, description, is_active, created_at, updated_at, priority) VALUES (9, 'Веб-разработка', 'Fullstack-разработка веб-приложений', true, '2026-09-19 13:04:41.179002', '2026-09-21 16:18:19.598857', 1);


--
-- TOC entry 5213 (class 0 OID 54541)
-- Dependencies: 224
-- Data for Name: it_products; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (1, 'Ростелеком Лицей', 'Ростелеком', 'Цифровая образовательная платформа «Ростелеком Лицей»', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (2, 'Виртуальная АТС', 'Ростелеком', 'Облачная телефония для бизнеса и учебных заведений', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (3, 'Цифровая АТС', 'Ростелеком', 'Телефония нового поколения для корпоративных клиентов', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (4, 'Wink', 'Ростелеком', 'Платформа онлайн-кинотеатра и стриминга', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (5, 'Видеонаблюдение Ростелеком', 'Ростелеком', 'Облачное видеонаблюдение для организаций', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (6, 'Яндекс Облако', 'Яндекс', 'Публичное облако: вычисления, хранение, ML-сервисы', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (7, '1С:Предприятие', '1С', 'Платформа автоматизации учёта и управления', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (8, 'Postgres Pro', 'Postgres Professional', 'СУБД PostgreSQL корпоративного уровня', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (9, 'Контур.Экстерн', 'СКБ Контур', 'Сдача отчётности и документооборот с контролирующими органами', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_products (it_products_id, name, vendor, description, is_active, created_at, updated_at) VALUES (10, 'VK Cloud', 'VK', 'Облачная платформа и инфраструктурные сервисы', true, '2026-09-19 13:04:41.179002', NULL);


--
-- TOC entry 5223 (class 0 OID 54609)
-- Dependencies: 234
-- Data for Name: it_programs; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (1, 1, 'DevOps-инженер', 'Учебная программа «DevOps-инженер»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (2, 1, 'Инженер по автоматизации CI/CD', 'Учебная программа «Инженер по автоматизации CI/CD»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (3, 2, 'Тестировщик ПО (QA)', 'Учебная программа «Тестировщик ПО (QA)»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (4, 2, 'Инженер по автоматизированному тестированию', 'Учебная программа «Инженер по автоматизированному тестированию»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (5, 3, 'Инженер данных', 'Учебная программа «Инженер данных»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (6, 3, 'Аналитик данных', 'Учебная программа «Аналитик данных»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (7, 4, 'Python-разработчик', 'Учебная программа «Python-разработчик»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (8, 4, 'Java-разработчик', 'Учебная программа «Java-разработчик»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (9, 4, 'Разработчик C# / .NET', 'Учебная программа «Разработчик C# / .NET»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (10, 5, 'Frontend-разработчик (React)', 'Учебная программа «Frontend-разработчик (React)»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (11, 5, 'Frontend-разработчик (Vue)', 'Учебная программа «Frontend-разработчик (Vue)»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (12, 6, 'ML-инженер', 'Учебная программа «ML-инженер»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (13, 6, 'Инженер компьютерного зрения', 'Учебная программа «Инженер компьютерного зрения»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (14, 7, 'Специалист по информационной безопасности', 'Учебная программа «Специалист по информационной безопасности»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (15, 7, 'Пентестер (этичный хакинг)', 'Учебная программа «Пентестер (этичный хакинг)»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (16, 8, 'Системный аналитик', 'Учебная программа «Системный аналитик»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (17, 8, 'Бизнес-аналитик в IT', 'Учебная программа «Бизнес-аналитик в IT»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (18, 9, 'Fullstack-веб-разработчик', 'Учебная программа «Fullstack-веб-разработчик»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (19, 9, 'Разработчик на 1С', 'Учебная программа «Разработчик на 1С»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (20, 10, 'Сетевой инженер', 'Учебная программа «Сетевой инженер»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (21, 10, 'Инженер телекоммуникационных сетей', 'Учебная программа «Инженер телекоммуникационных сетей»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.it_programs (it_programs_id, direction_id, name, description, is_active, created_at, updated_at) VALUES (22, 10, 'Администратор Linux', 'Учебная программа «Администратор Linux»: методические материалы и практика.', true, '2026-09-19 13:04:41.179002', NULL);


--
-- TOC entry 5221 (class 0 OID 54598)
-- Dependencies: 232
-- Data for Name: licenses; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (1, '2026-06-10 16:32:08', '2026-12-09 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (2, '2025-12-21 16:32:08', '2026-02-10 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (3, '2026-02-09 16:32:08', '2025-10-07 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (4, '2026-01-17 16:32:08', '2026-08-27 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (5, '2026-01-16 16:32:08', '2026-02-25 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (6, '2026-03-13 16:32:08', '2026-03-17 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (7, '2026-03-27 16:32:08', '2025-08-16 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (8, '2026-02-05 16:32:08', '2025-09-03 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (9, '2026-01-28 16:32:08', '2026-03-17 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (10, '2025-11-25 16:32:08', '2026-12-31 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (11, '2026-03-29 16:32:08', '2026-09-27 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (12, '2026-01-23 16:32:08', '2026-03-08 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (13, '2025-12-27 16:32:08', '2026-12-21 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (14, '2026-02-20 16:32:08', '2026-04-21 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (15, '2025-12-03 16:32:08', '2026-05-29 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (16, '2026-06-08 16:32:08', '2025-09-13 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (17, '2026-02-07 16:32:08', '2025-12-24 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (18, '2025-11-23 16:32:08', '2026-05-14 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (19, '2026-01-26 16:32:08', '2026-09-27 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (20, '2026-06-17 16:32:08', '2025-10-23 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (21, '2026-03-06 16:32:08', '2026-08-15 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (22, '2025-12-27 16:32:08', '2026-06-23 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (23, '2026-02-13 16:32:08', '2025-09-22 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (24, '2025-12-17 16:32:08', '2026-11-15 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (25, '2026-05-17 16:32:08', '2026-06-19 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (26, '2026-03-30 16:32:08', '2025-09-15 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (27, '2026-03-17 16:32:08', '2026-03-02 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (28, '2026-02-24 16:32:08', '2026-02-01 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (29, '2026-03-23 16:32:08', '2026-04-22 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (30, '2026-02-18 16:32:08', '2026-07-05 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (31, '2026-04-11 16:32:08', '2026-03-18 16:32:08', 'Передана', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (32, '2026-04-06 16:32:08', '2026-03-28 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (33, '2026-04-29 16:32:08', '2025-09-04 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (34, '2026-02-19 16:32:08', '2026-01-25 16:32:08', 'Требует продления', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');
INSERT INTO public.licenses (licenses_id, signed_at, valid_until, transfer_status, comment, created_at) VALUES (35, '2025-11-30 16:32:08', '2026-04-12 16:32:08', 'В процессе передачи', 'Лицензия ИТ-продукта (демо)', '2026-09-19 13:04:41.179002');


--
-- TOC entry 5231 (class 0 OID 54679)
-- Dependencies: 242
-- Data for Name: program_products; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (1, 1, 1);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (2, 2, 4);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (3, 2, 10);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (4, 3, 2);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (5, 4, 10);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (6, 5, 1);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (7, 5, 10);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (8, 6, 4);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (9, 7, 9);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (10, 8, 9);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (11, 9, 9);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (12, 10, 4);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (13, 10, 8);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (14, 11, 1);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (15, 11, 3);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (16, 12, 6);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (17, 12, 5);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (18, 13, 4);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (19, 14, 2);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (20, 14, 10);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (21, 15, 2);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (22, 15, 6);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (23, 16, 10);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (24, 16, 5);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (25, 17, 8);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (26, 18, 7);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (27, 19, 9);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (28, 20, 10);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (29, 20, 6);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (30, 21, 2);
INSERT INTO public.program_products (program_products_id, program_id, product_id) VALUES (31, 22, 4);


--
-- TOC entry 5211 (class 0 OID 54529)
-- Dependencies: 222
-- Data for Name: universities; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (1, 'Московский государственный университет имени М. В. Ломоносова', 'МГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (2, 'Санкт-Петербургский государственный университет', 'СПбГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (3, 'Московский физико-технический институт (национальный исследовательский университет)', 'МФТИ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (4, 'Национальный исследовательский ядерный университет «МИФИ»', 'МИФИ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (5, 'Московский государственный технический университет имени Н. Э. Баумана', 'МГТУ им. Баумана', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (6, 'Национальный исследовательский университет «МЭИ»', 'МЭИ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (7, 'Национальный исследовательский университет «МИЭТ»', 'МИЭТ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (8, 'Национальный исследовательский технологический университет «МИСиС»', 'МИСиС', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (9, 'Российский университет дружбы народов', 'РУДН', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (10, 'Национальный исследовательский университет «Высшая школа экономики»', 'НИУ ВШЭ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (11, 'Московский авиационный институт (национальный исследовательский университет)', 'МАИ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (12, 'Московский политехнический университет', 'Московский Политех', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (13, 'Московский государственный университет геодезии и картографии', 'МИИГАиК', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (14, 'Московский государственный университет печати имени Ивана Фёдорова', 'МГУП', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (15, 'Финансовый университет при Правительстве Российской Федерации', 'Финуниверситет', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (16, 'Российский экономический университет имени Г. В. Плеханова', 'РЭУ им. Плеханова', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (17, 'Российский государственный социальный университет', 'РГСУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (18, 'Государственный университет управления', 'ГУУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (19, 'Московский государственный юридический университет имени О. Е. Кутафина', 'МГЮА', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (20, 'Первый Московский государственный медицинский университет имени И. М. Сеченова', 'Сеченовский университет', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (21, 'Санкт-Петербургский политехнический университет Петра Великого', 'СПбПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (22, 'Санкт-Петербургский государственный электротехнический университет «ЛЭТИ»', 'СПбГЭТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (23, 'Университет ИТМО', 'ИТМО', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (24, 'Санкт-Петербургский государственный университет аэрокосмического приборостроения', 'ГУАП', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (25, 'Санкт-Петербургский государственный морской технический университет', 'СПбГМТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (26, 'Санкт-Петербургский государственный университет промышленных технологий и дизайна', 'СПбГУПТД', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (27, 'Санкт-Петербургский государственный экономический университет', 'СПбГЭУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (28, 'Балтийский государственный технический университет «ВОЕНМЕХ» имени Д. Ф. Устинова', 'БГТУ «ВОЕНМЕХ»', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (29, 'Российский государственный педагогический университет имени А. И. Герцена', 'Герценовский университет', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (30, 'Санкт-Петербургский государственный университет телекоммуникаций имени профессора М. А. Бонч-Бруевича', 'СПбГУТ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (31, 'Новосибирский государственный университет', 'НГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (32, 'Новосибирский государственный технический университет', 'НГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (33, 'Томский государственный университет', 'ТГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (34, 'Томский политехнический университет', 'ТПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (35, 'Томский государственный университет систем управления и радиоэлектроники', 'ТУСУР', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (36, 'Сибирский государственный университет телекоммуникаций и информатики', 'СибГУТИ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (37, 'Сибирский федеральный университет', 'СФУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (38, 'Сибирский государственный университет науки и технологий имени академика М. Ф. Решетнёва', 'СибГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (39, 'Иркутский государственный университет', 'ИГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (40, 'Иркутский национальный исследовательский технический университет', 'ИРНИТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (41, 'Иркутский государственный лингвистический университет', 'ИГЛУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (42, 'Уральский федеральный университет имени первого Президента России Б. Н. Ельцина', 'УрФУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (43, 'Уральский государственный педагогический университет', 'УрГПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (44, 'Южно-Уральский государственный университет (национальный исследовательский университет)', 'ЮУрГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (45, 'Челябинский государственный университет', 'ЧелГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (46, 'Челябинский государственный педагогический университет', 'ЧГПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (47, 'Пермский государственный национальный исследовательский университет', 'ПГНИУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (48, 'Пермский национальный исследовательский политехнический университет', 'ПНИПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (49, 'Казанский (Приволжский) федеральный университет', 'КФУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (50, 'Казанский национальный исследовательский технический университет имени А. Н. Туполева', 'КНИТУ-КАИ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (51, 'Казанский государственный энергетический университет', 'КГЭУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (52, 'Казанский национальный исследовательский технологический университет', 'КНИТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (53, 'Нижегородский государственный университет имени Н. И. Лобачевского', 'ННГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (54, 'Нижегородский государственный технический университет имени Р. Е. Алексеева', 'НГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (55, 'Нижегородский государственный архитектурно-строительный университет', 'ННГАСУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (56, 'Волгоградский государственный технический университет', 'ВолгГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (57, 'Волгоградский государственный университет', 'ВолГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (58, 'Волгоградский государственный социально-педагогический университет', 'ВГСПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (59, 'Самарский государственный технический университет', 'СамГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (60, 'Самарский государственный университет', 'СамГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (61, 'Самарский национальный исследовательский университет имени академика С. П. Королёва', 'СНИУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (62, 'Саратовский государственный университет имени Н. Г. Чернышевского', 'СГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (63, 'Саратовский государственный технический университет имени Ю. А. Гагарина', 'СГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (64, 'Ульяновский государственный технический университет', 'УлГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (65, 'Ульяновский государственный университет', 'УлГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (66, 'Пензенский государственный технологический университет', 'ПГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (67, 'Пензенский государственный университет', 'ПГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (68, 'Тамбовский государственный университет имени Г. Р. Державина', 'ТГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (69, 'Тамбовский государственный технический университет', 'ТГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (70, 'Липецкий государственный технический университет', 'ЛГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (71, 'Воронежский государственный университет', 'ВГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (72, 'Воронежский государственный технический университет', 'ВГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (73, 'Воронежский государственный аграрный университет имени императора Петра I', 'ВГАУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (74, 'Белгородский государственный технологический университет имени В. Г. Шухова', 'БГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (75, 'Белгородский государственный университет', 'БелГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (76, 'Курский государственный технический университет', 'КГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (77, 'Брянский государственный технический университет', 'БрГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (78, 'Орловский государственный технический университет', 'ОрГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (79, 'Тульский государственный университет', 'ТулГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (80, 'Тульский государственный педагогический университет имени Л. Н. Толстого', 'ТГПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (81, 'Рязанский государственный радиотехнический университет', 'РГРТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (82, 'Рязанский государственный университет имени С. А. Есенина', 'РГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (83, 'Государственный социально-гуманитарный университет', 'ГСГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (84, 'Ивановский государственный химико-технологический университет', 'ИГХТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (85, 'Ивановский государственный энергетический университет', 'ИГЭУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (86, 'Ярославский государственный технический университет', 'ЯрГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (87, 'Ярославский государственный педагогический университет имени К. Д. Ушинского', 'ЯГПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (88, 'Костромской государственный технологический университет', 'КГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (89, 'Владимирский государственный университет имени Александра Григорьевича и Николая Григорьевича Столетовых', 'ВлГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (90, 'Петрозаводский государственный университет', 'ПетрГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (91, 'Северный (Арктический) федеральный университет имени М. В. Ломоносова', 'САФУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (92, 'Мурманский арктический государственный университет', 'МАГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (93, 'Калининградский государственный технический университет', 'КГТУ-Калининград', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (94, 'Балтийский федеральный университет имени Иммануила Канта', 'БФУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (95, 'Смоленский государственный университет', 'СмГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (96, 'Псковский государственный университет', 'ПсковГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (97, 'Новгородский государственный университет имени Ярослава Мудрого', 'НовГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (98, 'Тверской государственный университет', 'ТвГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (99, 'Тверской государственный технический университет', 'ТвГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (100, 'Калужский государственный университет имени К. Э. Циолковского', 'КГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (101, 'Северо-Кавказский федеральный университет', 'СКФУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (102, 'Кабардино-Балкарский государственный университет имени Х. М. Бербекова', 'КБГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (103, 'Дагестанский государственный университет', 'ДГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (104, 'Дагестанский государственный технический университет', 'ДГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (105, 'Чеченский государственный университет', 'ЧГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (106, 'Северо-Осетинский государственный университет имени К. Л. Хетагурова', 'СОГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (107, 'Ингушский государственный университет', 'ИнГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (108, 'Карачаево-Черкесский государственный университет имени У. Д. Алиева', 'КЧГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (109, 'Адыгейский государственный университет', 'АдыГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (110, 'Крымский федеральный университет имени В. И. Вернадского', 'КФУ-Симферополь', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (111, 'Севастопольский государственный университет', 'СевГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (112, 'Ростовский государственный университет', 'РГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (113, 'Донской государственный технический университет', 'ДГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (114, 'Южный федеральный университет', 'ЮФУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (115, 'Астраханский государственный университет', 'АГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (116, 'Астраханский государственный технический университет', 'АГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (117, 'Омский государственный технический университет', 'ОмГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (118, 'Омский государственный университет имени Ф. М. Достоевского', 'ОмГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (119, 'Кемеровский государственный университет', 'КемГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (120, 'Кузбасский государственный технический университет имени Т. Ф. Горбачёва', 'КузГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (121, 'Алтайский государственный технический университет имени И. И. Ползунова', 'АлтГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (122, 'Алтайский государственный университет', 'АлтГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (123, 'Новосибирский государственный архитектурно-строительный университет', 'НГАСУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (124, 'Красноярский государственный педагогический университет имени В. П. Астафьева', 'КГПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (125, 'Восточно-Сибирский государственный университет технологий и управления', 'ВСГУТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (126, 'Бурятский государственный университет имени Доржи Банзарова', 'БГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (127, 'Забайкальский государственный университет', 'ЗабГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (128, 'Тихоокеанский государственный университет', 'ТОГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (129, 'Дальневосточный федеральный университет', 'ДВФУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (130, 'Дальневосточный государственный технический университет', 'ДВГТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (131, 'Владивостокский государственный университет экономики и сервиса', 'ВГУЭС', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (132, 'Сахалинский государственный университет', 'СахГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (133, 'Амурский государственный университет имени Михаила Шолохова', 'АмГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (134, 'Северный государственный медицинский университет', 'СГМУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (135, 'Московский государственный университет культуры и искусств', 'МГУКИ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (136, 'Санкт-Петербургский государственный университет культуры и искусств', 'СПбГУКИ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (137, 'Санкт-Петербургский государственный химико-фармацевтический университет', 'СПбГХФУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (138, 'Сибирский государственный медицинский университет', 'СибГМУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (139, 'Оренбургский государственный университет', 'ОГУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (140, 'Оренбургский государственный педагогический университет', 'ОГПУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (141, 'Башкирский государственный университет', 'БГУ-Уфа', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (142, 'Уфимский государственный авиационный технический университет', 'УГАТУ', true, '2026-09-19 13:04:41.179002', NULL);
INSERT INTO public.universities (universities_id, name, short_name, is_active, created_at, updated_at) VALUES (143, 'Уфимский государственный нефтяной технический университет', 'УНИПРО', true, '2026-09-19 13:04:41.179002', NULL);


--
-- TOC entry 5227 (class 0 OID 54643)
-- Dependencies: 238
-- Data for Name: university_contacts; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (1, 1, 'Лебедева Ольга Николаевна', 'Директор института', 'ольга.лебедева@university.ru', '+7987120868', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (2, 1, 'Андреев Артём Романович', 'Директор института', 'артём.андреев@university.ru', '+7913871230', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (3, 2, 'Захаров Кирилл Романович', 'Заведующий кафедрой', 'кирилл.захаров@university.ru', '+7961938483', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (4, 3, 'Николаева Ксения Алексеевна', 'Проректор по цифровизации', 'ксения.николаева@university.ru', '+7936279418', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (5, 4, 'Белова Полина Сергеевна', 'Директор института', 'полина.белова@university.ru', '+7925137722', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (6, 5, 'Титова Ксения Владимировна', 'Заведующий кафедрой', 'ксения.титова@university.ru', '+7929548432', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (7, 5, 'Смирнов Сергей Сергеевич', 'Начальник управления цифровизации', 'сергей.смирнов@university.ru', '+7972065818', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (8, 6, 'Баранов Кирилл Павлович', 'Декан факультета ИТ', 'кирилл.баранов@university.ru', '+7915476583', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (9, 6, 'Орлова Ольга Алексеевна', 'Заместитель декана', 'ольга.орлова@university.ru', '+7928612220', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (10, 7, 'Егорова Ирина Павловна', 'Проректор по цифровизации', 'ирина.егорова@university.ru', '+7959517169', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (11, 8, 'Фёдоров Александр Михайлович', 'Руководитель центра компетенций', 'александр.фёдоров@university.ru', '+7981326765', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (12, 9, 'Степанова Анастасия Александровна', 'Заведующий кафедрой', 'анастасия.степанова@university.ru', '+7912437026', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (13, 10, 'Смирноваа Наталья Сергеевна', 'Руководитель учебно-методического отдела', 'наталья.смирноваа@university.ru', '+7993770370', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (14, 10, 'Михайлова Егор Андреевич', 'Заведующий кафедрой', 'егор.михайлова@university.ru', '+7957693979', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (15, 11, 'Петрова Иван Дмитриевич', 'Заведующий кафедрой', 'иван.петрова@university.ru', '+7932074176', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (16, 11, 'Соколова Илья Михайлович', 'Заведующий кафедрой', 'илья.соколова@university.ru', '+7902191066', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (17, 12, 'Сидоров Никита Дмитриевич', 'Заведующий кафедрой', 'никита.сидоров@university.ru', '+7959143903', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (18, 13, 'Поповаа Марина Андреевна', 'Руководитель учебно-методического отдела', 'марина.поповаа@university.ru', '+7974194548', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (19, 14, 'Титов Павел Николаевич', 'Заместитель декана', 'павел.титов@university.ru', '+7981908841', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (20, 15, 'Орлов Сергей Андреевич', 'Заведующий кафедрой', 'сергей.орлов@university.ru', '+7939997381', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (21, 16, 'Морозов Кирилл Иванович', 'Заведующий кафедрой', 'кирилл.морозов@university.ru', '+7918434500', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (22, 16, 'Кузнецоваа Ольга Александровна', 'Декан факультета ИТ', 'ольга.кузнецоваа@university.ru', '+7914965789', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (23, 17, 'Куликов Артём Николаевич', 'Декан факультета ИТ', 'артём.куликов@university.ru', '+7927358113', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (24, 18, 'Егорова Полина Алексеевна', 'Заместитель декана', 'полина.егорова@university.ru', '+7999164991', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (25, 19, 'Алексеев Дмитрий Михайлович', 'Декан факультета ИТ', 'дмитрий.алексеев@university.ru', '+7961959074', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (26, 20, 'Ивановаа Ирина Александровна', 'Проректор по цифровизации', 'ирина.ивановаа@university.ru', '+7922149597', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (27, 21, 'Семёнова Ксения Дмитриевна', 'Заведующий кафедрой', 'ксения.семёнова@university.ru', '+7902375453', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (28, 22, 'Поповаа Юлия Алексеевна', 'Заведующий кафедрой', 'юлия.поповаа@university.ru', '+7965004485', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (29, 22, 'Михайлов Михаил Иванович', 'Руководитель центра компетенций', 'михаил.михайлов@university.ru', '+7911156287', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (30, 23, 'Поповаа Ольга Дмитриевна', 'Заведующий кафедрой', 'ольга.поповаа@university.ru', '+7995449324', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (31, 23, 'Андреев Максим Андреевич', 'Руководитель центра компетенций', 'максим.андреев@university.ru', '+7953646552', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (32, 24, 'Степанова Анна Романовна', 'Директор института', 'анна.степанова@university.ru', '+7913253031', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (33, 24, 'Васильев Андрей Алексеевич', 'Директор института', 'андрей.васильев@university.ru', '+7936752576', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (34, 25, 'Егорова Марина Алексеевна', 'Декан факультета ИТ', 'марина.егорова@university.ru', '+7918106422', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (35, 26, 'Орлов Андрей Романович', 'Директор института', 'андрей.орлов@university.ru', '+7928412769', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (36, 26, 'Титова Анна Дмитриевна', 'Проректор по цифровизации', 'анна.титова@university.ru', '+7921604451', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (37, 27, 'Новикова Виктория Сергеевна', 'Декан факультета ИТ', 'виктория.новикова@university.ru', '+7957117837', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (38, 27, 'Сидорова Дарья Андреевна', 'Заведующий кафедрой', 'дарья.сидорова@university.ru', '+7916933742', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (39, 28, 'Новикова Анастасия Сергеевна', 'Начальник управления цифровизации', 'анастасия.новикова@university.ru', '+7971415846', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (40, 28, 'Орлов Егор Романович', 'Заведующий кафедрой', 'егор.орлов@university.ru', '+7953670896', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (41, 29, 'Сидоров Владимир Андреевич', 'Заведующий кафедрой', 'владимир.сидоров@university.ru', '+7986866294', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (42, 30, 'Лебедева Анастасия Александровна', 'Заведующий кафедрой', 'анастасия.лебедева@university.ru', '+7976507321', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (43, 30, 'Кузнецов Кирилл Владимирович', 'Заместитель декана', 'кирилл.кузнецов@university.ru', '+7996555772', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (44, 31, 'Егоров Алексей Михайлович', 'Директор института', 'алексей.егоров@university.ru', '+7902818692', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (45, 32, 'Николаев Егор Михайлович', 'Проректор по цифровизации', 'егор.николаев@university.ru', '+7974188992', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (46, 32, 'Титов Александр Павлович', 'Заведующий кафедрой', 'александр.титов@university.ru', '+7968235969', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (47, 33, 'Орлова Юлия Романовна', 'Проректор по цифровизации', 'юлия.орлова@university.ru', '+7959508103', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (48, 34, 'Николаева Ксения Егоровна', 'Директор института', 'ксения.николаева@university.ru', '+7993135534', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (49, 34, 'Захаров Алексей Михайлович', 'Директор института', 'алексей.захаров@university.ru', '+7971006810', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (50, 35, 'Титов Никита Иванович', 'Руководитель учебно-методического отдела', 'никита.титов@university.ru', '+7984585314', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (51, 35, 'Фёдорова Елена Алексеевна', 'Руководитель центра компетенций', 'елена.фёдорова@university.ru', '+7914940441', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (52, 36, 'Волков Андрей Александрович', 'Декан факультета ИТ', 'андрей.волков@university.ru', '+7938971465', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (53, 36, 'Кузнецова Полина Николаевна', 'Заведующий кафедрой', 'полина.кузнецова@university.ru', '+7979294272', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (54, 37, 'Иванов Сергей Николаевич', 'Заведующий кафедрой', 'сергей.иванов@university.ru', '+7929689889', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (55, 37, 'Кузнецова Илья Дмитриевич', 'Руководитель учебно-методического отдела', 'илья.кузнецова@university.ru', '+7928795419', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (56, 38, 'Кудрявцева Виктория Павловна', 'Руководитель учебно-методического отдела', 'виктория.кудрявцева@university.ru', '+7928963715', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (57, 38, 'Семёнов Кирилл Павлович', 'Руководитель учебно-методического отдела', 'кирилл.семёнов@university.ru', '+7935606980', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (58, 39, 'Козлов Илья Алексеевич', 'Руководитель центра компетенций', 'илья.козлов@university.ru', '+7962351868', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (59, 39, 'Лебедев Роман Егорович', 'Начальник управления цифровизации', 'роман.лебедев@university.ru', '+7932077581', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (60, 40, 'Смирнова Иван Николаевич', 'Декан факультета ИТ', 'иван.смирнова@university.ru', '+7938048838', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (61, 40, 'Соколова Александр Михайлович', 'Заместитель декана', 'александр.соколова@university.ru', '+7981098919', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (62, 41, 'Захаров Егор Павлович', 'Заведующий кафедрой', 'егор.захаров@university.ru', '+7984681284', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (63, 41, 'Алексеева Александр Николаевич', 'Руководитель центра компетенций', 'александр.алексеева@university.ru', '+7973768991', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (64, 42, 'Волковаа Анна Николаевна', 'Декан факультета ИТ', 'анна.волковаа@university.ru', '+7918190929', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (65, 42, 'Баранов Алексей Александрович', 'Директор института', 'алексей.баранов@university.ru', '+7976492066', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (66, 43, 'Орлов Роман Алексеевич', 'Заместитель декана', 'роман.орлов@university.ru', '+7952374158', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (67, 44, 'Смирнова Дмитрий Владимирович', 'Заведующий кафедрой', 'дмитрий.смирнова@university.ru', '+7911675419', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (68, 44, 'Семёнова Екатерина Александровна', 'Начальник управления цифровизации', 'екатерина.семёнова@university.ru', '+7933117636', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (69, 45, 'Поповаа Екатерина Ивановна', 'Директор института', 'екатерина.поповаа@university.ru', '+7963815034', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (70, 45, 'Соколова Ирина Алексеевна', 'Проректор по цифровизации', 'ирина.соколова@university.ru', '+7906233774', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (71, 46, 'Волков Максим Михайлович', 'Заведующий кафедрой', 'максим.волков@university.ru', '+7916059717', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (72, 46, 'Михайловаа Ольга Михайловна', 'Декан факультета ИТ', 'ольга.михайловаа@university.ru', '+7969937982', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (73, 47, 'Кузнецова Юлия Александровна', 'Заместитель декана', 'юлия.кузнецова@university.ru', '+7982770648', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (74, 47, 'Макаров Иван Егорович', 'Начальник управления цифровизации', 'иван.макаров@university.ru', '+7973955087', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (75, 48, 'Смирноваа Марина Ивановна', 'Заместитель декана', 'марина.смирноваа@university.ru', '+7956407373', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (76, 48, 'Попова Татьяна Ивановна', 'Заведующий кафедрой', 'татьяна.попова@university.ru', '+7987358385', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (77, 49, 'Николаев Алексей Иванович', 'Заведующий кафедрой', 'алексей.николаев@university.ru', '+7965334434', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (78, 49, 'Михайлова Кирилл Павлович', 'Декан факультета ИТ', 'кирилл.михайлова@university.ru', '+7994205394', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (79, 50, 'Гусев Владимир Павлович', 'Заведующий кафедрой', 'владимир.гусев@university.ru', '+7989234615', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (80, 51, 'Попова Светлана Андреевна', 'Заместитель декана', 'светлана.попова@university.ru', '+7936137284', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (81, 51, 'Макарова Марина Павловна', 'Руководитель центра компетенций', 'марина.макарова@university.ru', '+7976549757', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (82, 52, 'Павлова Светлана Алексеевна', 'Заведующий кафедрой', 'светлана.павлова@university.ru', '+7914231028', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (83, 52, 'Смирнова Алексей Андреевич', 'Заведующий кафедрой', 'алексей.смирнова@university.ru', '+7985638893', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (84, 53, 'Волкова Светлана Андреевна', 'Руководитель центра компетенций', 'светлана.волкова@university.ru', '+7926071208', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (85, 53, 'Смирнова Андрей Алексеевич', 'Декан факультета ИТ', 'андрей.смирнова@university.ru', '+7905901239', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (86, 54, 'Алексееваа Ольга Александровна', 'Директор института', 'ольга.алексееваа@university.ru', '+7989031491', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (87, 55, 'Смирнов Кирилл Иванович', 'Проректор по цифровизации', 'кирилл.смирнов@university.ru', '+7917722755', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (88, 55, 'Попова Дмитрий Сергеевич', 'Начальник управления цифровизации', 'дмитрий.попова@university.ru', '+7952429098', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (89, 56, 'Гусев Илья Павлович', 'Заместитель декана', 'илья.гусев@university.ru', '+7988427625', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (90, 57, 'Титова Светлана Михайловна', 'Декан факультета ИТ', 'светлана.титова@university.ru', '+7914486102', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (91, 57, 'Егорова Елена Сергеевна', 'Заведующий кафедрой', 'елена.егорова@university.ru', '+7922259379', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (92, 58, 'Кудрявцев Владимир Алексеевич', 'Декан факультета ИТ', 'владимир.кудрявцев@university.ru', '+7935833597', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (93, 59, 'Баранова Елена Романовна', 'Заведующий кафедрой', 'елена.баранова@university.ru', '+7954318855', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (94, 59, 'Смирнова Илья Романович', 'Начальник управления цифровизации', 'илья.смирнова@university.ru', '+7953386328', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (95, 60, 'Степанов Михаил Иванович', 'Проректор по цифровизации', 'михаил.степанов@university.ru', '+7986101803', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (96, 61, 'Ивановаа Марина Ивановна', 'Проректор по цифровизации', 'марина.ивановаа@university.ru', '+7908248238', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (97, 61, 'Михайловаа Татьяна Александровна', 'Проректор по цифровизации', 'татьяна.михайловаа@university.ru', '+7931348050', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (98, 62, 'Морозова Марина Павловна', 'Руководитель учебно-методического отдела', 'марина.морозова@university.ru', '+7954044882', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (99, 62, 'Титова Марина Дмитриевна', 'Руководитель учебно-методического отдела', 'марина.титова@university.ru', '+7967850944', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (100, 63, 'Васильев Алексей Владимирович', 'Заместитель декана', 'алексей.васильев@university.ru', '+7985835492', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (101, 63, 'Белова Мария Ивановна', 'Проректор по цифровизации', 'мария.белова@university.ru', '+7965234550', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (102, 64, 'Белов Александр Романович', 'Руководитель учебно-методического отдела', 'александр.белов@university.ru', '+7971909393', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (103, 64, 'Макаров Владимир Романович', 'Руководитель учебно-методического отдела', 'владимир.макаров@university.ru', '+7904415140', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (104, 65, 'Козлова Полина Егоровна', 'Руководитель учебно-методического отдела', 'полина.козлова@university.ru', '+7911484348', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (105, 65, 'Михайловаа Анастасия Егоровна', 'Начальник управления цифровизации', 'анастасия.михайловаа@university.ru', '+7951229419', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (106, 66, 'Соколов Иван Дмитриевич', 'Начальник управления цифровизации', 'иван.соколов@university.ru', '+7985896624', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (107, 66, 'Павлова Виктория Ивановна', 'Руководитель учебно-методического отдела', 'виктория.павлова@university.ru', '+7938663605', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (108, 67, 'Михайлова Андрей Дмитриевич', 'Директор института', 'андрей.михайлова@university.ru', '+7976702160', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (109, 68, 'Козлова Светлана Михайловна', 'Руководитель учебно-методического отдела', 'светлана.козлова@university.ru', '+7928491822', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (110, 68, 'Андреева Юлия Павловна', 'Заместитель декана', 'юлия.андреева@university.ru', '+7986398528', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (111, 69, 'Семёнова Ксения Андреевна', 'Заместитель декана', 'ксения.семёнова@university.ru', '+7906338112', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (112, 70, 'Захарова Ксения Романовна', 'Начальник управления цифровизации', 'ксения.захарова@university.ru', '+7981621086', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (113, 70, 'Соколова Никита Дмитриевич', 'Руководитель учебно-методического отдела', 'никита.соколова@university.ru', '+7919823994', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (114, 71, 'Новиков Егор Романович', 'Начальник управления цифровизации', 'егор.новиков@university.ru', '+7918876745', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (115, 71, 'Егорова Юлия Михайловна', 'Заместитель декана', 'юлия.егорова@university.ru', '+7916512415', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (116, 72, 'Алексееваа Мария Михайловна', 'Проректор по цифровизации', 'мария.алексееваа@university.ru', '+7935821419', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (117, 72, 'Попова Виктория Дмитриевна', 'Проректор по цифровизации', 'виктория.попова@university.ru', '+7983791137', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (118, 73, 'Сидорова Юлия Александровна', 'Директор института', 'юлия.сидорова@university.ru', '+7967288789', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (119, 73, 'Семёнов Егор Михайлович', 'Начальник управления цифровизации', 'егор.семёнов@university.ru', '+7923937176', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (120, 74, 'Захарова Анастасия Ивановна', 'Начальник управления цифровизации', 'анастасия.захарова@university.ru', '+7938736811', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (121, 75, 'Иванов Иван Алексеевич', 'Начальник управления цифровизации', 'иван.иванов@university.ru', '+7918410995', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (122, 75, 'Соколоваа Светлана Романовна', 'Заместитель декана', 'светлана.соколоваа@university.ru', '+7958664433', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (123, 76, 'Захаров Владимир Дмитриевич', 'Заведующий кафедрой', 'владимир.захаров@university.ru', '+7977022335', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (124, 76, 'Козлова Анна Романовна', 'Заместитель декана', 'анна.козлова@university.ru', '+7951135861', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (125, 77, 'Алексееваа Светлана Андреевна', 'Руководитель центра компетенций', 'светлана.алексееваа@university.ru', '+7934189885', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (126, 78, 'Михайлова Ольга Романовна', 'Декан факультета ИТ', 'ольга.михайлова@university.ru', '+7958395785', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (127, 78, 'Макаров Андрей Дмитриевич', 'Директор института', 'андрей.макаров@university.ru', '+7967970985', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (128, 79, 'Смирнова Павел Павлович', 'Директор института', 'павел.смирнова@university.ru', '+7925311242', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (129, 80, 'Козлова Юлия Дмитриевна', 'Руководитель учебно-методического отдела', 'юлия.козлова@university.ru', '+7913361510', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (130, 80, 'Лебедева Ксения Павловна', 'Руководитель центра компетенций', 'ксения.лебедева@university.ru', '+7917619862', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (131, 81, 'Соколов Иван Владимирович', 'Директор института', 'иван.соколов@university.ru', '+7977232293', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (132, 82, 'Куликова Анна Михайловна', 'Руководитель центра компетенций', 'анна.куликова@university.ru', '+7932060605', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (133, 83, 'Степанова Виктория Дмитриевна', 'Начальник управления цифровизации', 'виктория.степанова@university.ru', '+7901624315', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (134, 83, 'Алексеева Сергей Дмитриевич', 'Заведующий кафедрой', 'сергей.алексеева@university.ru', '+7993276065', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (135, 84, 'Смирнова Егор Михайлович', 'Начальник управления цифровизации', 'егор.смирнова@university.ru', '+7972661307', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (136, 84, 'Волковаа Виктория Алексеевна', 'Декан факультета ИТ', 'виктория.волковаа@university.ru', '+7964645180', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (137, 85, 'Семёнов Павел Дмитриевич', 'Руководитель центра компетенций', 'павел.семёнов@university.ru', '+7997017548', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (138, 85, 'Павлов Артём Дмитриевич', 'Руководитель учебно-методического отдела', 'артём.павлов@university.ru', '+7914558545', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (139, 86, 'Орлов Илья Сергеевич', 'Заведующий кафедрой', 'илья.орлов@university.ru', '+7914475523', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (140, 87, 'Лебедева Юлия Сергеевна', 'Декан факультета ИТ', 'юлия.лебедева@university.ru', '+7953427461', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (141, 88, 'Морозова Ольга Романовна', 'Декан факультета ИТ', 'ольга.морозова@university.ru', '+7921249314', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (142, 89, 'Семёнова Юлия Александровна', 'Начальник управления цифровизации', 'юлия.семёнова@university.ru', '+7951879109', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (143, 89, 'Гусев Сергей Егорович', 'Проректор по цифровизации', 'сергей.гусев@university.ru', '+7988520810', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (144, 90, 'Васильева Полина Павловна', 'Заведующий кафедрой', 'полина.васильева@university.ru', '+7909748499', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (145, 90, 'Петров Дмитрий Иванович', 'Заместитель декана', 'дмитрий.петров@university.ru', '+7972811298', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (146, 91, 'Кудрявцева Елена Дмитриевна', 'Руководитель центра компетенций', 'елена.кудрявцева@university.ru', '+7922101965', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (147, 91, 'Волкова Никита Николаевич', 'Директор института', 'никита.волкова@university.ru', '+7989481739', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (148, 92, 'Соколов Артём Николаевич', 'Руководитель учебно-методического отдела', 'артём.соколов@university.ru', '+7937942639', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (149, 92, 'Баранов Роман Николаевич', 'Проректор по цифровизации', 'роман.баранов@university.ru', '+7968159911', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (150, 93, 'Макарова Наталья Романовна', 'Руководитель учебно-методического отдела', 'наталья.макарова@university.ru', '+7912530497', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (151, 93, 'Попова Виктория Дмитриевна', 'Руководитель центра компетенций', 'виктория.попова@university.ru', '+7922006313', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (152, 94, 'Гусева Дарья Романовна', 'Заместитель декана', 'дарья.гусева@university.ru', '+7905825620', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (153, 94, 'Андреева Ольга Михайловна', 'Заведующий кафедрой', 'ольга.андреева@university.ru', '+7929089795', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (154, 95, 'Андреева Дарья Дмитриевна', 'Директор института', 'дарья.андреева@university.ru', '+7938199311', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (155, 96, 'Павлова Анна Сергеевна', 'Директор института', 'анна.павлова@university.ru', '+7956701311', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (156, 97, 'Новиков Роман Дмитриевич', 'Начальник управления цифровизации', 'роман.новиков@university.ru', '+7902539438', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (157, 97, 'Алексеева Ксения Николаевна', 'Руководитель учебно-методического отдела', 'ксения.алексеева@university.ru', '+7963640889', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (158, 98, 'Николаев Максим Александрович', 'Начальник управления цифровизации', 'максим.николаев@university.ru', '+7921834876', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (159, 98, 'Павлова Полина Романовна', 'Заместитель декана', 'полина.павлова@university.ru', '+7988416270', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (160, 99, 'Иванова Сергей Владимирович', 'Заместитель декана', 'сергей.иванова@university.ru', '+7915752087', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (161, 99, 'Соколоваа Марина Павловна', 'Директор института', 'марина.соколоваа@university.ru', '+7904699685', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (162, 100, 'Смирнова Анна Андреевна', 'Директор института', 'анна.смирнова@university.ru', '+7933302417', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (163, 100, 'Козлова Юлия Дмитриевна', 'Декан факультета ИТ', 'юлия.козлова@university.ru', '+7988225192', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (164, 101, 'Смирнова Илья Павлович', 'Руководитель центра компетенций', 'илья.смирнова@university.ru', '+7917661748', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (165, 102, 'Баранов Максим Владимирович', 'Заместитель декана', 'максим.баранов@university.ru', '+7978007741', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (166, 103, 'Петров Никита Сергеевич', 'Руководитель учебно-методического отдела', 'никита.петров@university.ru', '+7962478574', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (167, 103, 'Васильев Илья Николаевич', 'Заместитель декана', 'илья.васильев@university.ru', '+7992319479', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (168, 104, 'Орлова Анастасия Владимировна', 'Начальник управления цифровизации', 'анастасия.орлова@university.ru', '+7919564488', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (169, 104, 'Петроваа Екатерина Владимировна', 'Руководитель центра компетенций', 'екатерина.петроваа@university.ru', '+7924963933', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (170, 105, 'Волков Алексей Михайлович', 'Начальник управления цифровизации', 'алексей.волков@university.ru', '+7913972018', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (171, 106, 'Попова Иван Романович', 'Руководитель центра компетенций', 'иван.попова@university.ru', '+7963532644', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (172, 106, 'Куликов Иван Романович', 'Директор института', 'иван.куликов@university.ru', '+7951942411', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (173, 107, 'Степанова Полина Ивановна', 'Декан факультета ИТ', 'полина.степанова@university.ru', '+7907186390', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (174, 107, 'Кузнецова Елена Михайловна', 'Заместитель декана', 'елена.кузнецова@university.ru', '+7981688246', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (175, 108, 'Поповаа Екатерина Владимировна', 'Руководитель учебно-методического отдела', 'екатерина.поповаа@university.ru', '+7993531028', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (176, 108, 'Кудрявцева Ольга Владимировна', 'Проректор по цифровизации', 'ольга.кудрявцева@university.ru', '+7993895058', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (177, 109, 'Кудрявцев Иван Павлович', 'Начальник управления цифровизации', 'иван.кудрявцев@university.ru', '+7967255418', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (178, 110, 'Орлов Дмитрий Романович', 'Руководитель центра компетенций', 'дмитрий.орлов@university.ru', '+7916531293', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (179, 110, 'Захаров Михаил Алексеевич', 'Начальник управления цифровизации', 'михаил.захаров@university.ru', '+7962367376', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (180, 111, 'Степанова Ксения Сергеевна', 'Проректор по цифровизации', 'ксения.степанова@university.ru', '+7957319630', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (181, 112, 'Петроваа Елена Романовна', 'Заместитель декана', 'елена.петроваа@university.ru', '+7997070468', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (182, 112, 'Степанов Алексей Андреевич', 'Руководитель центра компетенций', 'алексей.степанов@university.ru', '+7984221313', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (183, 113, 'Новикова Елена Алексеевна', 'Проректор по цифровизации', 'елена.новикова@university.ru', '+7999833734', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (184, 114, 'Волковаа Наталья Михайловна', 'Заместитель декана', 'наталья.волковаа@university.ru', '+7923722056', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (185, 115, 'Волковаа Ирина Егоровна', 'Руководитель учебно-методического отдела', 'ирина.волковаа@university.ru', '+7907893063', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (186, 116, 'Семёнова Полина Михайловна', 'Директор института', 'полина.семёнова@university.ru', '+7984925917', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (187, 116, 'Степанова Марина Андреевна', 'Руководитель центра компетенций', 'марина.степанова@university.ru', '+7988745704', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (188, 117, 'Ивановаа Виктория Сергеевна', 'Заведующий кафедрой', 'виктория.ивановаа@university.ru', '+7925194357', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (189, 117, 'Куликов Павел Павлович', 'Проректор по цифровизации', 'павел.куликов@university.ru', '+7993091745', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (190, 118, 'Фёдоров Кирилл Иванович', 'Начальник управления цифровизации', 'кирилл.фёдоров@university.ru', '+7972538698', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (191, 118, 'Лебедева Полина Владимировна', 'Декан факультета ИТ', 'полина.лебедева@university.ru', '+7971893502', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (192, 119, 'Семёнова Ксения Дмитриевна', 'Руководитель центра компетенций', 'ксения.семёнова@university.ru', '+7931472908', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (193, 119, 'Васильев Никита Сергеевич', 'Начальник управления цифровизации', 'никита.васильев@university.ru', '+7905813037', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (194, 120, 'Михайлова Марина Ивановна', 'Декан факультета ИТ', 'марина.михайлова@university.ru', '+7911318832', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (195, 120, 'Новиков Егор Дмитриевич', 'Директор института', 'егор.новиков@university.ru', '+7936052478', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (196, 121, 'Гусев Иван Дмитриевич', 'Проректор по цифровизации', 'иван.гусев@university.ru', '+7989993490', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (197, 122, 'Поповаа Анастасия Егоровна', 'Начальник управления цифровизации', 'анастасия.поповаа@university.ru', '+7958201433', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (198, 123, 'Семёнова Виктория Сергеевна', 'Проректор по цифровизации', 'виктория.семёнова@university.ru', '+7997049927', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (199, 124, 'Смирноваа Анна Николаевна', 'Руководитель учебно-методического отдела', 'анна.смирноваа@university.ru', '+7907490791', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (200, 125, 'Петров Павел Дмитриевич', 'Руководитель центра компетенций', 'павел.петров@university.ru', '+7932739191', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (201, 125, 'Орлова Наталья Александровна', 'Руководитель центра компетенций', 'наталья.орлова@university.ru', '+7996679880', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (202, 126, 'Баранова Марина Романовна', 'Начальник управления цифровизации', 'марина.баранова@university.ru', '+7922058651', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (203, 127, 'Волков Дмитрий Андреевич', 'Декан факультета ИТ', 'дмитрий.волков@university.ru', '+7966202210', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (204, 127, 'Смирноваа Марина Алексеевна', 'Декан факультета ИТ', 'марина.смирноваа@university.ru', '+7935800144', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (205, 128, 'Смирнова Юлия Алексеевна', 'Проректор по цифровизации', 'юлия.смирнова@university.ru', '+7968330827', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (206, 128, 'Кудрявцева Ксения Владимировна', 'Начальник управления цифровизации', 'ксения.кудрявцева@university.ru', '+7989347057', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (207, 129, 'Петроваа Татьяна Дмитриевна', 'Заместитель декана', 'татьяна.петроваа@university.ru', '+7918224389', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (208, 129, 'Морозова Светлана Владимировна', 'Проректор по цифровизации', 'светлана.морозова@university.ru', '+7916501583', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (209, 130, 'Михайлова Егор Сергеевич', 'Руководитель учебно-методического отдела', 'егор.михайлова@university.ru', '+7968501590', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (210, 130, 'Андреев Егор Алексеевич', 'Декан факультета ИТ', 'егор.андреев@university.ru', '+7917815076', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (211, 131, 'Фёдорова Анна Сергеевна', 'Руководитель учебно-методического отдела', 'анна.фёдорова@university.ru', '+7903117788', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (212, 131, 'Макаров Павел Николаевич', 'Декан факультета ИТ', 'павел.макаров@university.ru', '+7928548448', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (213, 132, 'Кузнецов Андрей Павлович', 'Руководитель центра компетенций', 'андрей.кузнецов@university.ru', '+7976274512', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (214, 132, 'Семёнова Ольга Александровна', 'Начальник управления цифровизации', 'ольга.семёнова@university.ru', '+7989686324', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (215, 133, 'Соколова Татьяна Алексеевна', 'Руководитель учебно-методического отдела', 'татьяна.соколова@university.ru', '+7935791184', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (216, 133, 'Алексееваа Екатерина Дмитриевна', 'Начальник управления цифровизации', 'екатерина.алексееваа@university.ru', '+7918584017', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (217, 134, 'Кудрявцева Елена Романовна', 'Руководитель центра компетенций', 'елена.кудрявцева@university.ru', '+7962089496', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (218, 135, 'Фёдорова Ирина Владимировна', 'Заведующий кафедрой', 'ирина.фёдорова@university.ru', '+7914371891', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (219, 135, 'Семёнова Марина Александровна', 'Руководитель центра компетенций', 'марина.семёнова@university.ru', '+7997189302', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (220, 136, 'Кузнецоваа Наталья Михайловна', 'Проректор по цифровизации', 'наталья.кузнецоваа@university.ru', '+7916189037', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (221, 136, 'Куликов Егор Николаевич', 'Проректор по цифровизации', 'егор.куликов@university.ru', '+7926318841', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (222, 137, 'Петрова Павел Сергеевич', 'Начальник управления цифровизации', 'павел.петрова@university.ru', '+7928258130', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (223, 138, 'Петроваа Наталья Алексеевна', 'Начальник управления цифровизации', 'наталья.петроваа@university.ru', '+7926413349', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (224, 139, 'Петрова Михаил Дмитриевич', 'Директор института', 'михаил.петрова@university.ru', '+7935604524', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (225, 140, 'Волковаа Елена Павловна', 'Начальник управления цифровизации', 'елена.волковаа@university.ru', '+7923872269', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (226, 141, 'Поповаа Мария Александровна', 'Проректор по цифровизации', 'мария.поповаа@university.ru', '+7905440189', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (227, 141, 'Поповаа Виктория Михайловна', 'Декан факультета ИТ', 'виктория.поповаа@university.ru', '+7985859883', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (228, 142, 'Белов Михаил Иванович', 'Проректор по цифровизации', 'михаил.белов@university.ru', '+7903652149', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (229, 142, 'Куликов Иван Андреевич', 'Руководитель центра компетенций', 'иван.куликов@university.ru', '+7926244264', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (230, 143, 'Андреева Ксения Сергеевна', 'Руководитель центра компетенций', 'ксения.андреева@university.ru', '+7992781470', true, NULL);
INSERT INTO public.university_contacts (university_contacts_id, university_id, full_name, "position", email, phone, is_active, comment) VALUES (231, 143, 'Баранов Сергей Алексеевич', 'Руководитель учебно-методического отдела', 'сергей.баранов@university.ru', '+7933363534', true, NULL);


--
-- TOC entry 5229 (class 0 OID 54659)
-- Dependencies: 240
-- Data for Name: university_managers; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (1, 4, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (2, 8, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (3, 12, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (4, 16, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (5, 20, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (6, 24, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (7, 28, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (8, 32, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (9, 36, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (10, 40, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (11, 44, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (12, 48, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (13, 52, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (14, 56, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (15, 60, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (16, 64, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (17, 68, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (18, 72, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (19, 76, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (20, 80, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (21, 84, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (22, 88, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (23, 92, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (24, 96, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (25, 100, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (26, 104, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (27, 108, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (28, 112, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (29, 116, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (30, 120, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (31, 124, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (32, 128, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (33, 132, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (34, 136, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (35, 140, 6, '2026-09-19 13:04:41.179002', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (36, 3, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (37, 6, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (38, 9, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (39, 12, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (40, 15, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (41, 18, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (42, 21, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (43, 24, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (44, 27, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (45, 30, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (46, 33, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (47, 36, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (48, 39, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (49, 42, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (50, 45, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (51, 48, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (52, 51, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (53, 54, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (54, 57, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (55, 60, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (56, 63, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (57, 66, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (58, 69, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (59, 72, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (60, 75, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (61, 78, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (62, 81, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (63, 84, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (64, 87, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (65, 90, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (66, 93, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (67, 96, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (68, 99, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (69, 102, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (70, 105, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (71, 108, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (72, 111, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (73, 114, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (74, 117, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (75, 120, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (76, 123, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (77, 126, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (78, 129, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (79, 132, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (80, 135, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (81, 138, 1, '2026-09-19 13:10:08.34509', true);
INSERT INTO public.university_managers (university_managers_id, university_id, user_id, assigned_at, is_primary) VALUES (82, 141, 1, '2026-09-19 13:10:08.34509', true);


--
-- TOC entry 5215 (class 0 OID 54555)
-- Dependencies: 226
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.users (users_id, keycloak_user_id, email, is_active, created_at, updated_at, last_name, first_name, middle_name) VALUES (6, '5b29c757-25c8-4589-be0a-04c9cf0bbef7', 'manager1@example.com', true, '2026-09-19 09:43:20.524947', NULL, 'One', 'Manager', NULL);
INSERT INTO public.users (users_id, keycloak_user_id, email, is_active, created_at, updated_at, last_name, first_name, middle_name) VALUES (1, 'dd916e98-1ca3-4c26-ad31-effae84b8171', 'ivanovgrigori@example.net', true, '2026-09-17 06:36:15.930891', '2026-09-25 10:03:41.200511', 'Иванов', 'Иван Иванович', NULL);
INSERT INTO public.users (users_id, keycloak_user_id, email, is_active, created_at, updated_at, last_name, first_name, middle_name) VALUES (2, '6e72fa42-4fcb-4dfb-8327-a52f46e7f3de', 'annavgrigori@example.net', true, '2026-09-17 06:55:43.87887', '2026-09-25 10:03:41.200508', 'Иванова', 'Анна Ивановича', NULL);
INSERT INTO public.users (users_id, keycloak_user_id, email, is_active, created_at, updated_at, last_name, first_name, middle_name) VALUES (3, '7cbd29ee-0cfa-418b-a996-d479f4555cd4', 'fdg@df.com', true, '2026-09-17 16:18:58.798477', '2026-09-25 10:03:41.20051', 'Пуа', 'Пвав Пвап', NULL);
INSERT INTO public.users (users_id, keycloak_user_id, email, is_active, created_at, updated_at, last_name, first_name, middle_name) VALUES (4, '44274138-9de7-4402-9ec5-a02d432b2979', 'fdsfdg@df.com', true, '2026-09-17 16:34:41.129175', '2026-09-25 10:03:41.20051', 'uiou', 'uiouio', NULL);
INSERT INTO public.users (users_id, keycloak_user_id, email, is_active, created_at, updated_at, last_name, first_name, middle_name) VALUES (5, 'bb92729c-37f7-4ed0-a928-5e32dbeecc00', 'sdfsdf@gmail.com', true, '2026-09-17 17:05:36.001665', '2026-09-25 10:03:41.200511', 'ccxfvxd', 'xcvxcv', NULL);
INSERT INTO public.users (users_id, keycloak_user_id, email, is_active, created_at, updated_at, last_name, first_name, middle_name) VALUES (8, '53e86358-c9f1-4780-85a0-2ed12f439354', NULL, true, '2026-09-25 10:03:41.200592', NULL, NULL, NULL, NULL);
INSERT INTO public.users (users_id, keycloak_user_id, email, is_active, created_at, updated_at, last_name, first_name, middle_name) VALUES (9, 'aeb708fe-c012-4ab3-9f89-5b9ed6d27bcd', NULL, true, '2026-09-25 10:03:41.214912', '2026-09-25 10:08:03.461758', NULL, NULL, NULL);
INSERT INTO public.users (users_id, keycloak_user_id, email, is_active, created_at, updated_at, last_name, first_name, middle_name) VALUES (7, 'dceace1c-0917-436d-9299-14d8572d765d', 'admin@example.com', true, '2026-09-21 15:03:27.473286', '2026-09-25 12:10:12.603205', 'User', 'Admin', NULL);


--
-- TOC entry 5225 (class 0 OID 54626)
-- Dependencies: 236
-- Data for Name: workflow_statuses; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (1, 1, 'Поиск контактов ответственного в вузе', 'Этап 1 базового workflow', 1, true, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (2, 1, 'Коммуникация и уточнение актуальности программ', 'Этап 2 базового workflow', 2, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (3, 1, 'Организация встречи с представителями вуза', 'Этап 3 базового workflow', 3, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (4, 1, 'Обмен пакетом документов для подписания', 'Этап 4 базового workflow', 4, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (5, 1, 'Корректировка документов перед подписанием', 'Этап 5 базового workflow', 5, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (6, 1, 'Подписание документов', 'Этап 6 базового workflow', 6, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (7, 1, 'Передача материалов и лицензии ИТ-продукта', 'Этап 7 базового workflow', 7, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (8, 1, 'Сопровождение внедрения ИТ-продукта', 'Этап 8 базового workflow', 8, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (9, 1, 'Обучение преподавателей', 'Этап 9 базового workflow', 9, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (10, 1, 'Актуализация учебной программы', 'Этап 10 базового workflow', 10, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (11, 1, 'Ведение занятий', 'Этап 11 базового workflow', 11, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (12, 1, 'Актуализация документации и материалов', 'Этап 12 базового workflow', 12, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (13, 1, 'Повышение квалификации преподавателей', 'Этап 13 базового workflow', 13, false, false);
INSERT INTO public.workflow_statuses (workflow_statuses_id, workflow_id, name, description, sort_order, is_initial, is_final) VALUES (14, 1, 'Контроль исполнения', 'Этап 14 базового workflow', 14, false, true);


--
-- TOC entry 5233 (class 0 OID 54697)
-- Dependencies: 244
-- Data for Name: workflow_transitions; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (1, 1, 1, 2);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (2, 1, 2, 3);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (3, 1, 3, 4);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (4, 1, 4, 5);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (5, 1, 5, 6);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (6, 1, 6, 7);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (7, 1, 7, 8);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (8, 1, 8, 9);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (9, 1, 9, 10);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (10, 1, 10, 11);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (11, 1, 11, 12);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (12, 1, 12, 13);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (13, 1, 13, 14);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (14, 1, 2, 1);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (15, 1, 3, 2);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (16, 1, 4, 3);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (17, 1, 5, 4);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (18, 1, 6, 5);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (19, 1, 7, 6);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (20, 1, 8, 7);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (21, 1, 9, 8);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (22, 1, 10, 9);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (23, 1, 11, 10);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (24, 1, 12, 11);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (25, 1, 13, 12);
INSERT INTO public.workflow_transitions (workflow_transitions_id, workflow_id, from_status_id, to_status_id) VALUES (26, 1, 14, 13);


--
-- TOC entry 5217 (class 0 OID 54571)
-- Dependencies: 228
-- Data for Name: workflows; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.workflows (workflows_id, name, description, is_active, version, created_at, updated_at) VALUES (1, 'B2B Взаимодействие с вузом', 'Базовый workflow взаимодействия с вузом (п. 2 ТЗ)', true, 1, '2026-09-19 13:04:41.179002', NULL);


--
-- TOC entry 5267 (class 0 OID 0)
-- Dependencies: 249
-- Name: attachments_attachments_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.attachments_attachments_id_seq', 4, true);


--
-- TOC entry 5268 (class 0 OID 0)
-- Dependencies: 251
-- Name: audit_logs_audit_logs_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.audit_logs_audit_logs_id_seq', 45, true);


--
-- TOC entry 5269 (class 0 OID 0)
-- Dependencies: 229
-- Name: contracts_contracts_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.contracts_contracts_id_seq', 40, true);


--
-- TOC entry 5270 (class 0 OID 0)
-- Dependencies: 253
-- Name: import_batches_import_batches_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.import_batches_import_batches_id_seq', 1, false);


--
-- TOC entry 5271 (class 0 OID 0)
-- Dependencies: 247
-- Name: interaction_status_history_interaction_status_history_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.interaction_status_history_interaction_status_history_id_seq', 651, true);


--
-- TOC entry 5272 (class 0 OID 0)
-- Dependencies: 245
-- Name: interactions_interactions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.interactions_interactions_id_seq', 160, true);


--
-- TOC entry 5273 (class 0 OID 0)
-- Dependencies: 219
-- Name: it_directions_it_directions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.it_directions_it_directions_id_seq', 10, true);


--
-- TOC entry 5274 (class 0 OID 0)
-- Dependencies: 223
-- Name: it_products_it_products_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.it_products_it_products_id_seq', 10, true);


--
-- TOC entry 5275 (class 0 OID 0)
-- Dependencies: 233
-- Name: it_programs_it_programs_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.it_programs_it_programs_id_seq', 22, true);


--
-- TOC entry 5276 (class 0 OID 0)
-- Dependencies: 231
-- Name: licenses_licenses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.licenses_licenses_id_seq', 35, true);


--
-- TOC entry 5277 (class 0 OID 0)
-- Dependencies: 241
-- Name: program_products_program_products_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.program_products_program_products_id_seq', 31, true);


--
-- TOC entry 5278 (class 0 OID 0)
-- Dependencies: 221
-- Name: universities_universities_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.universities_universities_id_seq', 143, true);


--
-- TOC entry 5279 (class 0 OID 0)
-- Dependencies: 237
-- Name: university_contacts_university_contacts_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.university_contacts_university_contacts_id_seq', 231, true);


--
-- TOC entry 5280 (class 0 OID 0)
-- Dependencies: 239
-- Name: university_managers_university_managers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.university_managers_university_managers_id_seq', 82, true);


--
-- TOC entry 5281 (class 0 OID 0)
-- Dependencies: 225
-- Name: users_users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_users_id_seq', 9, true);


--
-- TOC entry 5282 (class 0 OID 0)
-- Dependencies: 235
-- Name: workflow_statuses_workflow_statuses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.workflow_statuses_workflow_statuses_id_seq', 14, true);


--
-- TOC entry 5283 (class 0 OID 0)
-- Dependencies: 243
-- Name: workflow_transitions_workflow_transitions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.workflow_transitions_workflow_transitions_id_seq', 26, true);


--
-- TOC entry 5284 (class 0 OID 0)
-- Dependencies: 227
-- Name: workflows_workflows_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.workflows_workflows_id_seq', 1, true);


--
-- TOC entry 5028 (class 2606 OID 54814)
-- Name: attachments attachments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attachments
    ADD CONSTRAINT attachments_pkey PRIMARY KEY (attachments_id);


--
-- TOC entry 5030 (class 2606 OID 54840)
-- Name: audit_logs audit_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_pkey PRIMARY KEY (audit_logs_id);


--
-- TOC entry 5006 (class 2606 OID 54596)
-- Name: contracts contracts_contract_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.contracts
    ADD CONSTRAINT contracts_contract_number_key UNIQUE (contract_number);


--
-- TOC entry 5008 (class 2606 OID 54594)
-- Name: contracts contracts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.contracts
    ADD CONSTRAINT contracts_pkey PRIMARY KEY (contracts_id);


--
-- TOC entry 5032 (class 2606 OID 54856)
-- Name: import_batches import_batches_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.import_batches
    ADD CONSTRAINT import_batches_pkey PRIMARY KEY (import_batches_id);


--
-- TOC entry 5026 (class 2606 OID 54783)
-- Name: interaction_status_history interaction_status_history_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interaction_status_history
    ADD CONSTRAINT interaction_status_history_pkey PRIMARY KEY (interaction_status_history_id);


--
-- TOC entry 5024 (class 2606 OID 54727)
-- Name: interactions interactions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_pkey PRIMARY KEY (interactions_id);


--
-- TOC entry 4984 (class 2606 OID 54527)
-- Name: it_directions it_directions_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.it_directions
    ADD CONSTRAINT it_directions_name_key UNIQUE (name);


--
-- TOC entry 4986 (class 2606 OID 54525)
-- Name: it_directions it_directions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.it_directions
    ADD CONSTRAINT it_directions_pkey PRIMARY KEY (it_directions_id);


--
-- TOC entry 4992 (class 2606 OID 54553)
-- Name: it_products it_products_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.it_products
    ADD CONSTRAINT it_products_name_key UNIQUE (name);


--
-- TOC entry 4994 (class 2606 OID 54551)
-- Name: it_products it_products_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.it_products
    ADD CONSTRAINT it_products_pkey PRIMARY KEY (it_products_id);


--
-- TOC entry 5012 (class 2606 OID 54619)
-- Name: it_programs it_programs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.it_programs
    ADD CONSTRAINT it_programs_pkey PRIMARY KEY (it_programs_id);


--
-- TOC entry 5010 (class 2606 OID 54607)
-- Name: licenses licenses_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.licenses
    ADD CONSTRAINT licenses_pkey PRIMARY KEY (licenses_id);


--
-- TOC entry 5020 (class 2606 OID 54685)
-- Name: program_products program_products_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.program_products
    ADD CONSTRAINT program_products_pkey PRIMARY KEY (program_products_id);


--
-- TOC entry 4988 (class 2606 OID 54539)
-- Name: universities universities_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.universities
    ADD CONSTRAINT universities_name_key UNIQUE (name);


--
-- TOC entry 4990 (class 2606 OID 54537)
-- Name: universities universities_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.universities
    ADD CONSTRAINT universities_pkey PRIMARY KEY (universities_id);


--
-- TOC entry 5016 (class 2606 OID 54652)
-- Name: university_contacts university_contacts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.university_contacts
    ADD CONSTRAINT university_contacts_pkey PRIMARY KEY (university_contacts_id);


--
-- TOC entry 5018 (class 2606 OID 54667)
-- Name: university_managers university_managers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.university_managers
    ADD CONSTRAINT university_managers_pkey PRIMARY KEY (university_managers_id);


--
-- TOC entry 4996 (class 2606 OID 54569)
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- TOC entry 4998 (class 2606 OID 54567)
-- Name: users users_keycloak_user_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_keycloak_user_id_key UNIQUE (keycloak_user_id);


--
-- TOC entry 5000 (class 2606 OID 54565)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (users_id);


--
-- TOC entry 5014 (class 2606 OID 54636)
-- Name: workflow_statuses workflow_statuses_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflow_statuses
    ADD CONSTRAINT workflow_statuses_pkey PRIMARY KEY (workflow_statuses_id);


--
-- TOC entry 5022 (class 2606 OID 54703)
-- Name: workflow_transitions workflow_transitions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflow_transitions
    ADD CONSTRAINT workflow_transitions_pkey PRIMARY KEY (workflow_transitions_id);


--
-- TOC entry 5002 (class 2606 OID 54583)
-- Name: workflows workflows_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflows
    ADD CONSTRAINT workflows_name_key UNIQUE (name);


--
-- TOC entry 5004 (class 2606 OID 54581)
-- Name: workflows workflows_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflows
    ADD CONSTRAINT workflows_pkey PRIMARY KEY (workflows_id);


--
-- TOC entry 5056 (class 2606 OID 54815)
-- Name: attachments attachments_interaction_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attachments
    ADD CONSTRAINT attachments_interaction_id_fkey FOREIGN KEY (interaction_id) REFERENCES public.interactions(interactions_id);


--
-- TOC entry 5057 (class 2606 OID 54820)
-- Name: attachments attachments_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attachments
    ADD CONSTRAINT attachments_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.workflow_statuses(workflow_statuses_id);


--
-- TOC entry 5058 (class 2606 OID 54825)
-- Name: attachments attachments_uploaded_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.attachments
    ADD CONSTRAINT attachments_uploaded_by_fkey FOREIGN KEY (uploaded_by) REFERENCES public.users(users_id);


--
-- TOC entry 5059 (class 2606 OID 54841)
-- Name: audit_logs audit_logs_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(users_id);


--
-- TOC entry 5060 (class 2606 OID 54857)
-- Name: import_batches import_batches_uploaded_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.import_batches
    ADD CONSTRAINT import_batches_uploaded_by_fkey FOREIGN KEY (uploaded_by) REFERENCES public.users(users_id);


--
-- TOC entry 5052 (class 2606 OID 54799)
-- Name: interaction_status_history interaction_status_history_changed_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interaction_status_history
    ADD CONSTRAINT interaction_status_history_changed_by_fkey FOREIGN KEY (changed_by) REFERENCES public.users(users_id);


--
-- TOC entry 5053 (class 2606 OID 54789)
-- Name: interaction_status_history interaction_status_history_from_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interaction_status_history
    ADD CONSTRAINT interaction_status_history_from_status_id_fkey FOREIGN KEY (from_status_id) REFERENCES public.workflow_statuses(workflow_statuses_id);


--
-- TOC entry 5054 (class 2606 OID 54784)
-- Name: interaction_status_history interaction_status_history_interaction_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interaction_status_history
    ADD CONSTRAINT interaction_status_history_interaction_id_fkey FOREIGN KEY (interaction_id) REFERENCES public.interactions(interactions_id);


--
-- TOC entry 5055 (class 2606 OID 54794)
-- Name: interaction_status_history interaction_status_history_to_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interaction_status_history
    ADD CONSTRAINT interaction_status_history_to_status_id_fkey FOREIGN KEY (to_status_id) REFERENCES public.workflow_statuses(workflow_statuses_id);


--
-- TOC entry 5043 (class 2606 OID 54763)
-- Name: interactions interactions_contract_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_contract_id_fkey FOREIGN KEY (contract_id) REFERENCES public.contracts(contracts_id);


--
-- TOC entry 5044 (class 2606 OID 54758)
-- Name: interactions interactions_current_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_current_status_id_fkey FOREIGN KEY (current_status_id) REFERENCES public.workflow_statuses(workflow_statuses_id);


--
-- TOC entry 5045 (class 2606 OID 54768)
-- Name: interactions interactions_license_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_license_id_fkey FOREIGN KEY (license_id) REFERENCES public.licenses(licenses_id);


--
-- TOC entry 5046 (class 2606 OID 54743)
-- Name: interactions interactions_manager_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_manager_id_fkey FOREIGN KEY (manager_id) REFERENCES public.users(users_id);


--
-- TOC entry 5047 (class 2606 OID 54738)
-- Name: interactions interactions_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.it_products(it_products_id);


--
-- TOC entry 5048 (class 2606 OID 54733)
-- Name: interactions interactions_program_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_program_id_fkey FOREIGN KEY (program_id) REFERENCES public.it_programs(it_programs_id);


--
-- TOC entry 5049 (class 2606 OID 54748)
-- Name: interactions interactions_university_contact_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_university_contact_id_fkey FOREIGN KEY (university_contact_id) REFERENCES public.university_contacts(university_contacts_id);


--
-- TOC entry 5050 (class 2606 OID 54728)
-- Name: interactions interactions_university_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_university_id_fkey FOREIGN KEY (university_id) REFERENCES public.universities(universities_id);


--
-- TOC entry 5051 (class 2606 OID 54753)
-- Name: interactions interactions_workflow_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.interactions
    ADD CONSTRAINT interactions_workflow_id_fkey FOREIGN KEY (workflow_id) REFERENCES public.workflows(workflows_id);


--
-- TOC entry 5033 (class 2606 OID 54620)
-- Name: it_programs it_programs_direction_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.it_programs
    ADD CONSTRAINT it_programs_direction_id_fkey FOREIGN KEY (direction_id) REFERENCES public.it_directions(it_directions_id);


--
-- TOC entry 5038 (class 2606 OID 54691)
-- Name: program_products program_products_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.program_products
    ADD CONSTRAINT program_products_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.it_products(it_products_id);


--
-- TOC entry 5039 (class 2606 OID 54686)
-- Name: program_products program_products_program_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.program_products
    ADD CONSTRAINT program_products_program_id_fkey FOREIGN KEY (program_id) REFERENCES public.it_programs(it_programs_id);


--
-- TOC entry 5035 (class 2606 OID 54653)
-- Name: university_contacts university_contacts_university_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.university_contacts
    ADD CONSTRAINT university_contacts_university_id_fkey FOREIGN KEY (university_id) REFERENCES public.universities(universities_id);


--
-- TOC entry 5036 (class 2606 OID 54668)
-- Name: university_managers university_managers_university_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.university_managers
    ADD CONSTRAINT university_managers_university_id_fkey FOREIGN KEY (university_id) REFERENCES public.universities(universities_id);


--
-- TOC entry 5037 (class 2606 OID 54673)
-- Name: university_managers university_managers_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.university_managers
    ADD CONSTRAINT university_managers_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(users_id);


--
-- TOC entry 5034 (class 2606 OID 54637)
-- Name: workflow_statuses workflow_statuses_workflow_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflow_statuses
    ADD CONSTRAINT workflow_statuses_workflow_id_fkey FOREIGN KEY (workflow_id) REFERENCES public.workflows(workflows_id);


--
-- TOC entry 5040 (class 2606 OID 54709)
-- Name: workflow_transitions workflow_transitions_from_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflow_transitions
    ADD CONSTRAINT workflow_transitions_from_status_id_fkey FOREIGN KEY (from_status_id) REFERENCES public.workflow_statuses(workflow_statuses_id);


--
-- TOC entry 5041 (class 2606 OID 54714)
-- Name: workflow_transitions workflow_transitions_to_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflow_transitions
    ADD CONSTRAINT workflow_transitions_to_status_id_fkey FOREIGN KEY (to_status_id) REFERENCES public.workflow_statuses(workflow_statuses_id);


--
-- TOC entry 5042 (class 2606 OID 54704)
-- Name: workflow_transitions workflow_transitions_workflow_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.workflow_transitions
    ADD CONSTRAINT workflow_transitions_workflow_id_fkey FOREIGN KEY (workflow_id) REFERENCES public.workflows(workflows_id);


-- Completed on 2026-09-25 22:09:34

--
-- PostgreSQL database dump complete
--

\unrestrict EDLGEmT8apZFj9eqSX8eO3dU3OjC87Xtcp9hMetYsLHRcFGZ9rqfuoTMgoNChTs

